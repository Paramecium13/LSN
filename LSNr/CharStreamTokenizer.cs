using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokens;
using Tokens.Tokens;

namespace LSNr
{
	public class CharStreamTokenizer
	{
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

		protected IToken PreviousToken;

		protected readonly StringBuilder StrB = new StringBuilder();

		protected readonly StringBuilder UEscStrB = new StringBuilder();

		private readonly Action<IToken> TokenOutput;


		protected void ReadChar(char c)
		{

		}

		protected void BaseReadChar(char c)
		{

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

		protected void SymReadChar(char c)
		{
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
					break;
				case TokenizerState.SymbolAnd:
					break;
				case TokenizerState.SymbolOr:
					break;
				case TokenizerState.SymbolAsterisk:
					break;
				case TokenizerState.SymbolLess:
					break;
				case TokenizerState.SymbolGreater:
					break;
				case TokenizerState.SybolAt:
					break;
				case TokenizerState.SymbolSlash:
					break;
				case TokenizerState.SymbolMinus:
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
			var str = StrB.ToString();
			IToken token;
			switch (tokenType)
			{
				case TokenType.Unknown:
					throw new ApplicationException();
				case TokenType.Word:
					throw new NotImplementedException();
				case TokenType.Float:
					token = new FloatToken(str);
					break;
				case TokenType.Int:
					token = new IntToken(str);
					break;
				case TokenType.String:
					token = new StringToken(str);
					break;
				case TokenType.Assignment:
					token = new Assignment(str);
					break;
				case TokenType.Operator:
					token = new Operator(str);
					break;
				case TokenType.SyntaxSymbol:
					token = new SyntaxSymbol(str);
					break;
				default:
					throw new ApplicationException();
			}
			PreviousToken = token;
			TokenOutput(token);
			tokenType = TokenType.Unknown;
			State = TokenizerState.Base;
			StrB.Clear();
		}


	}
}
