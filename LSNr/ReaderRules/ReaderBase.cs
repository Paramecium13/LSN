using LsnCore.Utilities;
using System.Collections.Generic;

namespace LSNr
{
	public abstract class ReaderBase
	{
		private readonly ISlice<Token> Tokens;

		int TokenIndex;
		int CurrentHeadStart;
		int CurrentHeadCount;
		int CurrentBodyStart;
		int CurrentBodyCount;
		int Balance;

		IReadToken TokenReader;

		protected ReaderBase(ISlice<Token> tokens) {
			Tokens = tokens;
			TokenReader = AttrBaseReader;
		}

		/// <summary>
		/// When a group of tokens ending with a semicolon has been read.
		/// </summary>
		/// <param name="tokens">The tokens before the semicolon.</param>
		protected abstract void OnReadStatement(ISlice<Token> tokens, ISlice<Token>[] attributes);

		/// <summary>
		/// When a body (tokens enclosed in braces) has been read.
		/// </summary>
		/// <param name="headTokens">The tokens before the opening brace, e.g. a function signature...</param>
		/// <param name="bodyTokens">The tokens between the braces.</param>
		protected abstract void OnReadBody(ISlice<Token> headTokens, ISlice<Token> bodyTokens, ISlice<Token>[] attributes);

		/// <summary>
		/// When reading adjacent semicolons, or (maybe?) a semicolon after a closing brace.
		/// </summary>
		protected abstract void OnReadAdjSemiColon(ISlice<Token>[] attributes);

		protected void ReadTokens()
		{
			for (TokenIndex = 0; TokenIndex < Tokens.Count; TokenIndex++)
				TokenReader.Read(Tokens[TokenIndex], this);
		}

		ISlice<Token> GetHeader() => Slice<Token>.Create(Tokens, CurrentHeadStart, CurrentBodyStart-CurrentHeadStart);

		ISlice<Token> GetStatement() => Slice<Token>.Create(Tokens, CurrentHeadStart, CurrentHeadCount);

		void ResetHead()
		{
			CurrentHeadStart = TokenIndex + 1;
			CurrentHeadCount = -1;
			CurrentBodyCount = 0;
		}

		ISlice<Token>[] PopAttributes()
		{
			var attributes = CurrentAttributes.ToArray();
			CurrentAttributes.Clear();
			return attributes;
		}

		readonly IReadToken AttrBaseReader = new BaseAttributeReadToken(new string[] { "unique"});

		interface IReadToken
		{
			void Read(Token token, ReaderBase reader);
		}

		class StatementReadToken : IReadToken
		{
			internal static readonly StatementReadToken Instance = new StatementReadToken();
			StatementReadToken() { }
			public void Read(Token token, ReaderBase reader)
			{
				reader.CurrentHeadCount++;
				if(token.Type == TokenType.SyntaxSymbol)
				{
					switch (token.Value)
					{
						case ";":
							if (reader.CurrentHeadCount != -1)
								reader.OnReadStatement(reader.GetStatement(), reader.PopAttributes());
							else
								reader.OnReadAdjSemiColon(reader.PopAttributes());
							reader.ResetHead();
							reader.CurrentHeadCount++;
							reader.TokenReader = reader.AttrBaseReader;
							return;
						case "{":
							reader.CurrentBodyStart = reader.TokenIndex + 1;
							// The body does not include the opening '{'.
							reader.Balance = 1;
							reader.TokenReader = BodyReadToken.Instance;
							return;
						default:
							break;
					}
				}
			}
		}

		class BodyReadToken : IReadToken
		{
			internal static readonly BodyReadToken Instance = new BodyReadToken();
			BodyReadToken() { }
			public void Read(Token token, ReaderBase reader)
			{
				if (token.Type == TokenType.SyntaxSymbol)
				{
					switch (token.Value)
					{
						case "{":
							reader.Balance++;
							break;
						case "}":
							//...
							reader.Balance--;
							if (reader.Balance == 0)
							{
								var head = reader.GetHeader();
								reader.OnReadBody(head, Slice<Token>.Create(reader.Tokens, reader.CurrentBodyStart, reader.CurrentBodyCount), reader.PopAttributes());
								reader.ResetHead();
								// I don't need to change body[start/count] or balance b/c these will be changed before it switches to this state.
								reader.TokenReader = reader.AttrBaseReader;
								return;
							}
							break;
						default:
							break;
					}
				}
				reader.CurrentBodyCount++;
			}
		}

		List<ISlice<Token>> CurrentAttributes = new List<ISlice<Token>>();

		class BaseAttributeReadToken : IReadToken
		{
			readonly HashSet<string> FreeAttributeNames;

			public BaseAttributeReadToken(IEnumerable<string> freeAttrNames)
			{
				FreeAttributeNames = new HashSet<string>(freeAttrNames);
			}

			public void Read(Token token, ReaderBase reader)
			{
				if (token.Type == TokenType.SyntaxSymbol && token.Value == "[")
				{
					// start reading attribute.
					reader.TokenReader = new AttributeReadToken(reader.TokenIndex + 1, this);
				}
				else if (FreeAttributeNames.Contains(token.Value))
				{
					// push token onto current attributes.
					reader.CurrentHeadCount = 1;
					reader.CurrentAttributes.Add(reader.GetStatement());
					reader.ResetHead();
				}
				else
				{
					reader.TokenIndex--;
					reader.TokenReader = StatementReadToken.Instance;
				}
			}
		}

		class AttributeReadToken : IReadToken
		{
			/// <summary>
			/// The index of the token right after the opening '['.
			/// </summary>
			readonly int Start;
			int Count;
			int Balance = 1;
			IReadToken BaseAttrReadToken;

			public AttributeReadToken(int start, IReadToken baseReadToken)
			{
				Start = start;
				BaseAttrReadToken = baseReadToken;
			}

			public void Read(Token token, ReaderBase reader)
			{
				if (token.Type == TokenType.SyntaxSymbol)
				{
					switch (token.Value)
					{
						case "[":
							Balance++;
							Count++;
							break;
						case "]":
							Balance--;
							if (Balance == 0)
							{
								reader.CurrentAttributes.Add(Slice<Token>.Create(reader.Tokens, Start, Count));
								reader.TokenReader = BaseAttrReadToken;
							}
							else Count++;
							break;
						default:
							Count++;
							break;
					}
				}
				else Count++;
			}
		}
	}
}
