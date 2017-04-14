using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokens;
using Tokens.Tokens;

namespace LSNr
{
	public class CharStreamTokenizer
	{

		private readonly static char[] OtherOperators = new char[] {
			'^','~','∈','∊','∋','∍','⊂','⊃'
		};

		private readonly static char[] Symbols = new char[] {
			'+','-','*','/','%',/*'^'*/'>','<','~','!',/*':',*/'?','@','$','=','|','&'
		};

		private readonly static char[] SyntaxSymbols = new char[] {
			'(',')','{','}','[',']',',',';',':','`'
		};

		private static readonly string[] Keywords = new string[]
		{
			//Core Language stuff
			"if", "else","elsif","let","mut","unless","struct","fn","for","match",
			"foreach","return","new","choose","turn","quest","virtual","stage","state",
			"record","repeat","is",

			//Statement headers
			"give","rotate","publish",/*"add","remove",*/"callcommonevent","cce","recover",
			"goto","open","fadein","fadeout","tint","flash","shake","play","show","end","exit",
			"wait","start","stop","change","say","turn",//move

			//Statement stuff
			"item","weapon","armor","armour","actor","video","image","soundeffect","backgroundmusic",
			"screen","moveroute","animation","picture","with","as","down","left","right","up","graphic",
			
			//both
			"hp","level","exp","mp","g",//"name",

			//Get expression stuff
			"get","mapid","playtime","savecount","battlecount","number","of","keyitem","timer",
			"input","at",

			//Things
			//"my","common","all"
			//"int","double","num","complex","bool","string",
			"watched","rule","when","scriptobject","attachedto","attatched","to","host","hostinterface",
			"interface","script","property","state","auto","setstate","on","event","abstract","virtual","override"
		};

		protected enum TokenizerState
		{
			Begin,
			Base,
			Word,
			Number,
			Decimal,
			CommentSingleLine,
			CommentMultiLine,
			/// <summary>
			/// When a * is seen in a multiline comment.
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
			SymbolLess,
			SymbolGreater,
			SybolAt,
			SymbolSlash,
			SymbolExclamation,
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

		protected enum TokenType
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

		protected TokenType tokenType;

		protected TokenizerState State;

		//protected IToken PreviousToken;

		protected bool CanBeNegativeSign = false;

		protected readonly StringBuilder StrB = new StringBuilder();

		protected readonly StringBuilder UEscStrB = new StringBuilder();

		private readonly Action<IToken> TokenOutput;

		private readonly List<IToken> Tokens;

		private int LineNumber = 1;


		internal CharStreamTokenizer()
		{
			Tokens = new List<IToken>();
			TokenOutput = (t) => Tokens.Add(t);
		}


		internal IReadOnlyList<IToken> Tokenize(string src)
		{
			int lngth = src.Length;
			for (int i = 0; i < lngth; i++)
				ReadChar(src[i]);
			return Tokens;
		}


		protected void ReadChar(char c)
		{
			if (c == '\n') LineNumber++;
			if (State < TokenizerState.CommentSingleLine)
				BaseReadChar(c);
			else if (State < TokenizerState.SymbolPlus)
				CommentReadChar(c);
			else if (State < TokenizerState.StringBase)
				SymReadChar(c);
			else StrReadChar(c);
			
		}

		protected void BaseReadChar(char c)
		{
			if(char.IsWhiteSpace(c))
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
				tokenType = TokenType.Operator;
				Push(c);
				Pop();
				return;
			}
			if (SyntaxSymbols.Contains(c))
			{
				Pop();
				tokenType = TokenType.SyntaxSymbol;
				Push(c);
				Pop();
				return;
			}
			if(c == '"')
			{
				Pop();
				State = TokenizerState.StringBase;
				tokenType = TokenType.String;
				return;
			}
			switch (State)
			{
				case TokenizerState.Begin:
					if (char.IsDigit(c))
					{
						Push(c);
						State = TokenizerState.Number;
						tokenType = TokenType.Int;
					}
					else if (c == '.')
					{
						Push('.');
						tokenType = TokenType.SyntaxSymbol;
						Pop();
					}
					else
					{
						Push(c);
						State = TokenizerState.Word;
						tokenType = TokenType.Word;
					}
					break;
				case TokenizerState.Base:
					if(char.IsDigit(c))
					{
						Push(c);
						State = TokenizerState.Number;
						tokenType = TokenType.Int;
					}
					else if (c == '.')
					{
						Push('.');
						tokenType = TokenType.SyntaxSymbol;
						Pop();
					}
					else
					{
						Push(c);
						State = TokenizerState.Word;
						tokenType = TokenType.Word;
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
						Push('.');
						tokenType = TokenType.SyntaxSymbol;
						Pop();
					}
					else Push(c);
					break;
				case TokenizerState.Number:
					if (Char.IsDigit(c))
						Push(c);
					else if (c == '.')
					{
						Push(c);
						State = TokenizerState.Decimal;
						tokenType = TokenType.Float;
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
				default:
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
			tokenType = TokenType.Operator;
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
					State = TokenizerState.SybolAt;
					break;
				default:
					break;
			}
		}

		protected void SymReadChar(char c)
		{
			if (c == '"')
			{
				Pop();
				State = TokenizerState.StringBase;
				tokenType = TokenType.String;
				return;
			}
			switch (State)
			{
				case TokenizerState.SymbolPlus:
					if (c == '=')
					{
						Push('+');
						tokenType = TokenType.Assignment;
						Pop();
					}
					else if (c == '+')
					{
						Push('+');
						tokenType = TokenType.Assignment;
						Pop();
					}
					else
					{
						tokenType = TokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolEquals:
					if(c == '=')
					{
						Push('=');
						tokenType = TokenType.Operator;
						Pop();
					}
					else
					{
						tokenType = TokenType.Assignment;
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolPercent:
					if (c == '=')
					{
						Push('=');
						tokenType = TokenType.Assignment;
						Pop();
					}
					else
					{
						tokenType = TokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolAnd:
					break;
				case TokenizerState.SymbolOr:
					break;
				case TokenizerState.SymbolAsterisk:
					if (c == '=')
					{
						Push('=');
						tokenType = TokenType.Assignment;
						Pop();
					}
					else
					{
						tokenType = TokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolLess:
					tokenType = TokenType.Operator;
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
					if(c=='\"')
					{
						throw new NotImplementedException();
					}
					break;
				case TokenizerState.SymbolSlash:
					if (c == '=')
					{
						Push('=');
						tokenType = TokenType.Assignment;
						Pop();
					}
					else if (c == '/')
					{
						State = TokenizerState.CommentSingleLine;
						tokenType = TokenType.Unknown;
						StrB.Clear();
					}
					else if (c == '*')
					{
						State = TokenizerState.CommentMultiLine;
						tokenType = TokenType.Unknown;
						StrB.Clear();
					}
					else
					{
						tokenType = TokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
				case TokenizerState.SymbolExclamation:
					tokenType = TokenType.Operator;
					if (c == '=')
					{
						Push('=');
						Pop();
					}
					else Pop();
					break;
				case TokenizerState.SymbolMinus:
					if (c == '=')
					{
						Push('=');
						tokenType = TokenType.Assignment;
						Pop();
					}
					else if (c == '-')
					{
						Push('-');
						tokenType = TokenType.Assignment;
						Pop();
					}
					else if (c == '>')
					{
						Push('>');
						tokenType = TokenType.SyntaxSymbol;
						Pop();
					}
					else if(CanBeNegativeSign && char.IsDigit(c))
					{
						Push(c);
						State = TokenizerState.Number;
					}
					else
					{
						tokenType = TokenType.Operator;
						Pop();
						BaseReadChar(c);
					}
					break;
				default:
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
					int i;
					if (int.TryParse(UEscStrB.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out i))
						Push((char)i);
					else
						throw new ApplicationException();
					UEscStrB.Clear();
					State = TokenizerState.StringBase;
					break;
				default:
					break;
			}
		}

		protected void CommentReadChar(char c)
		{
			switch (State)
			{
				case TokenizerState.CommentSingleLine:
					if (c == '\n')
						State = TokenizerState.Base;
					break;
				case TokenizerState.CommentMultiLine:
					if (c == '*')/**/
						State = TokenizerState.CommentMultiLineStar;
					break;
				case TokenizerState.CommentMultiLineStar:
					if (c == '/')
						State = TokenizerState.Base;
					else
						State = TokenizerState.CommentMultiLine;
					break;
				default:
					throw new ApplicationException();
			}
		}

		protected void Push(char c)
		{
			StrB.Append(c);
		}

		protected void Pop()
		{
			CanBeNegativeSign = false;
			var str = StrB.ToString();
			IToken token;
			switch (tokenType)
			{
				case TokenType.Unknown:
					if(string.IsNullOrEmpty(str))
					{
						tokenType = TokenType.Unknown;
						State = TokenizerState.Base;
						StrB.Clear();
						return;
					}
					throw new ApplicationException();
				case TokenType.Word:
					if (Keywords.Contains(str.ToLower()))
						token = new Keyword(str.ToLower(), LineNumber);
					else
						token = new Identifier(str, LineNumber);
					break;
				case TokenType.Float:
					token = new FloatToken(str, LineNumber);
					break;
				case TokenType.Int:
					token = new IntToken(str, LineNumber);
					break;
				case TokenType.String:
					token = new StringToken(str, LineNumber);
					break;
				case TokenType.Assignment:
					token = new Assignment(str, LineNumber);
					CanBeNegativeSign = true;
					break;
				case TokenType.Operator:
					token = new Operator(str, LineNumber);
					CanBeNegativeSign = true;
					break;
				case TokenType.SyntaxSymbol:
					token = new SyntaxSymbol(str, LineNumber);
					CanBeNegativeSign = true;
					break;
				default:
					throw new ApplicationException();
			}
			//PreviousToken = token;
			TokenOutput(token);
			tokenType = TokenType.Unknown;
			State = TokenizerState.Base;
			StrB.Clear();
		}


	}
}
