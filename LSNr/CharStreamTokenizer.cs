﻿using LsnCore.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LSNr
{
	public class CharStreamTokenizer
	{
		private static readonly char[] OtherOperators = {
			'^','~','∈','∊','∋','∍','⊂','⊃'
		};

		private static readonly char[] Symbols = {
			'+','-','*','/','%','>','<','~','!','?','@',/*'$',*/'=','|','&'
		};

		private static readonly char[] SyntaxSymbols = {
			'(',')','{','}','[',']',',',';',':','`'
		};

		private static readonly string[] Keywords = {
			//"turn","quest","stage","repeat","unless",

			// Statement headers:
			"let","return","give","goto","wait","attach","detach","say","set","setstate",
			"comefrom",
			//"exit","start","stop","change","fadein","fadeout","tint","flash","shake",
			//"play","show","end","rotate","publish",

			//Statement stuff:
			"mut","item","gold","with","as","graphic","is","to",
			//"soundeffect","backgroundmusic","video","image","down","left","right","up","screen",
			//"moveroute","animation","picture",

			// Control Structures:
			"if","choose","when","else","elsif","for","match","foreach",

			// Expressions:
			"new","host","self","true","false","none","some",
			//Things

			// Types:
			"script","class","scriptclass","interface","hostinterface","struct","record",
			"string","int","double",
			// Type Contents:
			"fn","property","state","auto","on","event","abstract","virtual", "conversation", "node", "branch", "start", "prompt",
			"end","endconversation", "action"

			//both
			//"hp","level","exp","mp","g",

			//Get expression stuff
			//"get","number","of","at",

			//"my","common","all"
			//"watched","rule","attachedto","attatched","override"
		};

		protected enum TokenizerState
		{
			Base,
			Word,
			Number,
			Decimal,
			CommentSingleline,
			CommentMultiline,
			/// <summary>
			/// When a * is seen in a multi-line comment.
			/// </summary>
			CommentMultiLineStar,

			/// <summary>
			/// The first symbol state.
			/// </summary>
			SymbolPlus,
			SymbolEquals,
			SymbolPercent,
			SymbolAnd,
			SymbolOr,
			SymbolAsterisk,

			/// <summary>
			/// Midpoint
			/// </summary>
			SymbolLess,
			SymbolGreater,
			SybolAt,
			SymbolSlash,
			SymbolExclamation,
			SymbolNumberDot,
			SymbolNonNumDot,
			/// <summary>
			/// The last symbol state.
			/// </summary>
			SymbolMinus,

			StringBase,
			StringEsc,
			StringU0,
			StringU1,
			StringU2,
			StringU3
		}

		protected enum TokenizerTokenType
		{
			Unknown,
			/// <summary>
			/// Identifier or keyword.
			/// </summary>
			Word,
			Float,
			Int,
			String,
			Assignment,
			Operator,
			SyntaxSymbol
		}

		protected TokenizerTokenType TokenType;

		protected TokenizerState State;

		//protected IToken PreviousToken;

		protected bool CanBeNegativeSign;

		protected readonly StringBuilder StrB = new StringBuilder();

		protected readonly StringBuilder UEscStrB = new StringBuilder();

		private readonly Action<Token> TokenOutput;

		private readonly List<Token> Tokens;

		private int LineNumber = 1;

		internal CharStreamTokenizer()
		{
			Tokens = new List<Token>();
			TokenOutput = (t) => Tokens.Add(t);
		}

		internal ISlice<Token> Tokenize(string src)
		{
			var lngth = src.Length;
			for (int i = 0; i < lngth; i++)
				ReadChar(src[i]);
			return Tokens.ToSlice();
		}

		protected void ReadChar(char c)
		{
			if (c == '\n') LineNumber++;
			if (State < TokenizerState.CommentSingleline)
				BaseReadChar(c);
			else if (State < TokenizerState.SymbolPlus)
				CommentReadChar(c);
			else if (State < TokenizerState.SymbolLess)
				Symbols1ReadChar(c);
			else if (State < TokenizerState.StringBase)
				Symbols2ReadChar(c);
			else StrReadChar(c);
		}

		/// <summary>
		/// Read a character when the current token is relatively simple...
		/// </summary>
		/// <param name="c"></param>
		protected void BaseReadChar(char c)
		{
			if (char.IsWhiteSpace(c))
			{
				Pop();
				return;
			}
			if (Symbols.Contains(c))
			{
				Pop();
				SymReadInitChar(c);
				return;
			}
			if (OtherOperators.Contains(c)) // It's a one char operator.
			{
				Pop();
				TokenType = TokenizerTokenType.Operator;
				Push(c);
				Pop();
				return;
			}
			if (SyntaxSymbols.Contains(c))
			{
				Pop();
				TokenType = TokenizerTokenType.SyntaxSymbol;
				Push(c);
				Pop();
				return;
			}
			if (c == '"')
			{
				Pop();
				State = TokenizerState.StringBase;
				TokenType = TokenizerTokenType.String;
				return;
			}
			switch (State)
			{
				case TokenizerState.Base:
					if (char.IsDigit(c))
					{
						Push(c);
						State = TokenizerState.Number;
						TokenType = TokenizerTokenType.Int;
					}
					else if (c == '.')
					{
						Push(c);
						State = TokenizerState.SymbolNonNumDot;
						TokenType = TokenizerTokenType.SyntaxSymbol;
					}
					else
					{
						Push(c);
						State = TokenizerState.Word;
						TokenType = TokenizerTokenType.Word;
					}
					break;
				case TokenizerState.Word:
					if (char.IsWhiteSpace(c))
					{
						Pop();
					}
					else if (c == '.')
					{
						Pop();
						Push(c);
						TokenType = TokenizerTokenType.SyntaxSymbol;
						State = TokenizerState.SymbolNonNumDot;
					}
					else Push(c);
					break;
				case TokenizerState.Number:
					if (char.IsDigit(c))
						Push(c);
					else if (c == '.')
					{
						State = TokenizerState.SymbolNumberDot;
					}
					else if (char.IsWhiteSpace(c))
					{
						Pop();
					}
					else
						throw new ApplicationException();
					break;
				case TokenizerState.Decimal:
					if (Char.IsDigit(c))
						Push(c);
					else if (char.IsWhiteSpace(c))
					{
						Pop();
					}
					else
						throw new ApplicationException();
					break;
			}
		}

		/// <summary>
		/// Reads the first symbol char in a possible long symbol.
		/// </summary>
		/// <param name="c"></param>
		protected void SymReadInitChar(char c)
		{
			Push(c);
			TokenType = TokenizerTokenType.Operator;
			switch (c)
			{
				//'~',':','?','@','$'
				case '+':
					State = TokenizerState.SymbolPlus;
					break;
				case '-':
					State = TokenizerState.SymbolMinus;
					break;
				case '*':
					State = TokenizerState.SymbolAsterisk;
					break;
				case '/':
					State = TokenizerState.SymbolSlash;
					break;
				case '%':
					State = TokenizerState.SymbolPercent;
					break;
				case '>':
					State = TokenizerState.SymbolGreater;
					break;
				case '<':
					State = TokenizerState.SymbolLess;
					break;
				case '!':
					State = TokenizerState.SymbolExclamation;
					break;
				case '=':
					State = TokenizerState.SymbolEquals;
					break;
				case '|':
					State = TokenizerState.SymbolOr;
					break;
				case '&':
					State = TokenizerState.SymbolAnd;
					break;
				case '@':
					State = TokenizerState.SybolAt; //Is this even used?
					break;
				default:
					break;
			}
		}

		protected void Symbols1ReadChar(char c)
		{
			if (c == '"')
			{
				Pop();
				State = TokenizerState.StringBase;
				TokenType = TokenizerTokenType.String;
				return;
			}
			switch (State)
			{
				case TokenizerState.SymbolPlus:
					if (c == '=')
					{
						Push('+');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
					}
					else if (c == '+')
					{
						Push('+');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
					}
					else
					{
						TokenType = TokenizerTokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolEquals:
					if (c == '=')
					{
						Push('=');
						TokenType = TokenizerTokenType.Operator;
						Pop();
					}
					else
					{
						TokenType = TokenizerTokenType.Assignment;
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolPercent:
					if (c == '=')
					{
						Push('=');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
					}
					else
					{
						TokenType = TokenizerTokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolAnd:
					if (c == '&')
					{
						Push('&');
						TokenType = TokenizerTokenType.Operator;
						Pop();
						break;
					}
					if (c == '=')
					{
						Push('=');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
						break;
					}
					TokenType = TokenizerTokenType.Operator;
					Pop();
					BaseReadChar(c);
					break;
				case TokenizerState.SymbolOr:
					if (c == '|')
					{
						Push('|');
						TokenType = TokenizerTokenType.Operator;
						Pop();
						break;
					}
					if (c == '=')
					{
						Push('=');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
						break;
					}
					TokenType = TokenizerTokenType.Operator;
					Pop();
					BaseReadChar(c);
					break;
				case TokenizerState.SymbolAsterisk:
					if (c == '=')
					{
						Push('=');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
					}
					else
					{
						TokenType = TokenizerTokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
			}
		}

		protected void Symbols2ReadChar(char c)
		{
			switch (State)
			{
				case TokenizerState.SymbolLess:
					TokenType = TokenizerTokenType.Operator;
					if (c == '=')
					{
						Push('=');
						Pop();
					}
					else
					{
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolGreater:
					if (c == '=')
					{
						Push('=');
						Pop();
					}
					else
					{
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SybolAt:
					if (c == '\"')
					{
						throw new NotImplementedException();
					}
					break;
				case TokenizerState.SymbolSlash:
					switch (c)
					{
						case '=':
							Push('=');
							TokenType = TokenizerTokenType.Assignment;
							Pop();
							break;
						case '/':
							State = TokenizerState.CommentSingleline;
							TokenType = TokenizerTokenType.Unknown;
							StrB.Clear();
							break;
						case '*':
							State = TokenizerState.CommentMultiline;
							TokenType = TokenizerTokenType.Unknown;
							StrB.Clear();
							break;
						default:
							TokenType = TokenizerTokenType.Operator;
							Pop();
							BaseReadChar(c);
							break;
					}
					break;
				case TokenizerState.SymbolExclamation:
					TokenType = TokenizerTokenType.Operator;
					if (c == '=')
					{
						Push('=');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
					}
					else Pop();
					break;
				case TokenizerState.SymbolNumberDot:
					if (c == '.')
					{
						Pop();
						Push(c);
						Push(c);
						TokenType = TokenizerTokenType.Operator;
						Pop();
					}
					else if (char.IsDigit(c))
					{
						Push('.');
						Push(c);
						State = TokenizerState.Decimal;
						TokenType = TokenizerTokenType.Float;
					}
					else
						throw new ApplicationException();
					break;
				case TokenizerState.SymbolNonNumDot:
					if(c == '.')
					{
						Push(c);
						TokenType = TokenizerTokenType.Operator;
						Pop();
					}
					else
					{
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolMinus:
					if (c == '=')
					{
						Push('=');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
					}
					else if (c == '-')
					{
						Push('-');
						TokenType = TokenizerTokenType.Assignment;
						Pop();
					}
					else if (c == '>')
					{
						Push('>');
						TokenType = TokenizerTokenType.SyntaxSymbol;
						Pop();
					}
					else if (CanBeNegativeSign && char.IsDigit(c))
					{
						Push(c);
						State = TokenizerState.Number;
						TokenType = TokenizerTokenType.Int;
					}
					else
					{
						TokenType = TokenizerTokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
			}
		}

		protected void StrReadChar(char c)
		{
			switch (State)
			{
				case TokenizerState.StringBase:
					if (c == '\\')
						State = TokenizerState.StringEsc;
					else if(c == '\"')
					{
						Pop();
					}
					else
						Push(c);
					break;
				case TokenizerState.StringEsc:
					switch (c)
					{
						case '\'':
							Push('\'');
							State = TokenizerState.StringBase;
							break;
						case '\"':
							Push('\"');
							State = TokenizerState.StringBase;
							break;
						case '\\':
							Push('\\');
							State = TokenizerState.StringBase;
							break;
						case '0':
							Push('\0');
							State = TokenizerState.StringBase;
							break;
						case 'a':
							Push('\a');
							State = TokenizerState.StringBase;
							break;
						case 'f':
							Push('\f');
							State = TokenizerState.StringBase;
							break;
						case 'n':
							Push('\n');
							State = TokenizerState.StringBase;
							break;
						case 'r':
							Push('\r');
							State = TokenizerState.StringBase;
							break;
						case 't':
							Push('\t');
							State = TokenizerState.StringBase;
							break;
						case 'v':
							Push('\v');
							State = TokenizerState.StringBase;
							break;
						case 'u':
							State = TokenizerState.StringU0;
							break;
						default:
							throw new ApplicationException();
					}
					break;
				case TokenizerState.StringU0:
					UEscStrB.Append(c);
					State = TokenizerState.StringU1;
					break;
				case TokenizerState.StringU1:
					UEscStrB.Append(c);
					State = TokenizerState.StringU2;
					break;
				case TokenizerState.StringU2:
					UEscStrB.Append(c);
					State = TokenizerState.StringU3;
					break;
				case TokenizerState.StringU3:
					UEscStrB.Append(c);
					if (int.TryParse(UEscStrB.ToString(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var i))
						Push((char)i);
					else
						throw new ApplicationException();
					UEscStrB.Clear();
					State = TokenizerState.StringBase;
					break;
			}
		}

		protected void CommentReadChar(char c)
		{
			switch (State)
			{
				case TokenizerState.CommentSingleline:
					if (c == '\n')
						State = TokenizerState.Base;
					break;
				case TokenizerState.CommentMultiline:
					if (c == '*')/**/
						State = TokenizerState.CommentMultiLineStar;
					break;
				case TokenizerState.CommentMultiLineStar:
					if (c == '/')
						State = TokenizerState.Base;
					else
						State = TokenizerState.CommentMultiline;
					break;
				default:
					throw new ApplicationException();
			}
		}

		/// <summary>
		/// Add the character to the token being parsed.
		/// </summary>
		/// <param name="c"></param>
		protected void Push(char c)
		{
			StrB.Append(c);
		}

		/// <summary>
		/// Emit the current token.
		/// </summary>
		protected void Pop()
		{
			var str = StrB.ToString();
			if(!string.IsNullOrEmpty(str))
				CanBeNegativeSign = false;
			Token token;
			switch (TokenType)
			{
				case TokenizerTokenType.Unknown:
					if (!string.IsNullOrEmpty(str)) throw new ApplicationException();
					TokenType = TokenizerTokenType.Unknown;
					State = TokenizerState.Base;
					StrB.Clear();
					return;
				case TokenizerTokenType.Word:
					if (Keywords.Contains(str.ToLower()))
						token = new Token(str.ToLower(), LineNumber, LSNr.TokenType.Keyword);
					else if (str.StartsWith("$", StringComparison.Ordinal))
						token = new Token(str, LineNumber, LSNr.TokenType.GameValue);
					else
						token = new Token(str, LineNumber, LSNr.TokenType.Identifier);
					break;
				case TokenizerTokenType.Float:
					token = new Token(LineNumber, double.Parse(str, CultureInfo.InvariantCulture));
					break;
				case TokenizerTokenType.Int:
					token = new Token(LineNumber, int.Parse(str));
					break;
				case TokenizerTokenType.String:
					token = new Token(str, LineNumber, LSNr.TokenType.String);
					break;
				case TokenizerTokenType.Assignment:
					token = new Token(str, LineNumber, LSNr.TokenType.Assignment);
					CanBeNegativeSign = true;
					break;
				case TokenizerTokenType.Operator:
					token = new Token(str, LineNumber, LSNr.TokenType.Operator);
					CanBeNegativeSign = true;
					break;
				case TokenizerTokenType.SyntaxSymbol:
					token = new Token(str, LineNumber, LSNr.TokenType.SyntaxSymbol);
					switch (str)
					{
						case ",":
						case "`":
						case ";":
						case ":":
							CanBeNegativeSign = true;
							break;
						default:
							break;
					}
					break;
				default:
					throw new ApplicationException();
			}
			//PreviousToken = token;
			TokenOutput(token);
			TokenType = TokenizerTokenType.Unknown;
			State = TokenizerState.Base;
			StrB.Clear();
		}


	}
}
