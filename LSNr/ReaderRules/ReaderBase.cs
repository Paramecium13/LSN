﻿using LsnCore.Utilities;

namespace LSNr
{
	abstract class ReaderBase
	{
		private readonly ISlice<Token> Tokens;

		int TokenIndex;
		int CurrentHeadStart;
		int CurrentHeadCount;
		int CurrentBodyStart;
		int CurrentBodyCount;
		int Balance;

		IReadToken TokenReader = DefaultReadToken.Instance;

		protected ReaderBase(ISlice<Token> tokens) { Tokens = tokens; }

		/// <summary>
		/// When a group of tokens ending with a semicolon has been read.
		/// </summary>
		/// <param name="tokens">The tokens before the semicolon.</param>
		protected abstract void OnReadStatement(ISlice<Token> tokens);

		/// <summary>
		/// When a body (tokens enclosed in braces) has been read.
		/// </summary>
		/// <param name="headTokens">The tokens before the opening brace, e.g. a function signature...</param>
		/// <param name="bodyTokens">The tokens between the braces.</param>
		protected abstract void OnReadBody(ISlice<Token> headTokens, ISlice<Token> bodyTokens);

		/// <summary>
		/// When reading adjacent semicolons, or (maybe?) a semicolon after a closing brace.
		/// </summary>
		protected abstract void OnReadAdjSemiColon();

		protected void ReadTokens()
		{
			for (TokenIndex = 0; TokenIndex < Tokens.Count; TokenIndex++)
				TokenReader.Read(Tokens[TokenIndex], this);
		}

		ISlice<Token> GetHeadB() => Slice<Token>.Create(Tokens, CurrentHeadStart, CurrentBodyStart-CurrentHeadStart);

		ISlice<Token> GetHead() => Slice<Token>.Create(Tokens, CurrentHeadStart, CurrentHeadCount);

		interface IReadToken
		{
			void Read(Token token, ReaderBase reader);
		}

		class DefaultReadToken : IReadToken
		{
			internal static readonly DefaultReadToken Instance = new DefaultReadToken();
			DefaultReadToken() { }
			public void Read(Token token, ReaderBase reader)
			{
				reader.CurrentHeadCount++;
				if(token.Type == TokenType.SyntaxSymbol)
				{
					switch (token.Value)
					{
						case ";":
							if (reader.CurrentHeadCount != -1)
								reader.OnReadStatement(reader.GetHead());
							else
								reader.OnReadAdjSemiColon();
							reader.CurrentHeadStart = reader.TokenIndex + 1;
							reader.CurrentHeadCount = -1;
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
								var head = reader.GetHeadB();
								reader.OnReadBody(head, Slice<Token>.Create(reader.Tokens, reader.CurrentBodyStart, reader.CurrentBodyCount));
								reader.CurrentHeadStart = reader.TokenIndex + 1;
								reader.CurrentHeadCount = -1;
								reader.CurrentBodyCount = 0;
								// I don't need to change body[start/count] or balance b/c these will be changed before it switches to this state.
								reader.TokenReader = DefaultReadToken.Instance;
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
	}
}
