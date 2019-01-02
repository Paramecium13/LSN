using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	class SomeRule : IExpressionRule
	{
		internal static readonly SomeRule Rule = new SomeRule();

		SomeRule() { }

		public uint Priority => ExpressionRulePriorities.MemberAccess;

		public bool CheckToken(Token token, IPreScript script)
			=> token.Type == TokenType.Keyword && token.Value == "some";

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			// some		(			...		)
			// ^index	^index+1
			var ls = new List<Token>();
			var j = index + 1;
			var balance = 1;
			while (balance != 0)
			{
				++j;
				var t = tokens[j];
				if (t.Value == "(")
				{
					++balance;
					ls.Add(t);
				}
				else if (t.Value == ")")
				{
					--balance;
					if (balance != 0)
						ls.Add(t);
				}
				else ls.Add(t);
			}
			// some		(			...		)
			// ^index	^index+1			^j

			return (new SomeExpression(ExpressionParser.Parse(ls.ToArray(), script, substitutions)), j + 1, 0);
		}
	}
}
