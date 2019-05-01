using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LSNr.LssParser
{
	public class IndexerRule : IExpressionRule
	{
		public uint Priority => ExpressionRulePriorities.MemberAccess;

		public bool CheckToken(Token token, IPreScript script)
			=> token.Type == TokenType.SyntaxSymbol && token.Value == "[";

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var leftExpr = substitutions[tokens[index - 1]];
			var colType = leftExpr.Type.Type as ICollectionType;
			if (colType == null)
				throw new LsnrParsingException(tokens[index], $"{leftExpr.Type.Name} is not a collection type so it cannot be indexed.", script.Path);
			var j = index;
			var ls = new List<Token>();
			var balance = 1;
			while (balance != 0)
			{
				++j;
				var t = tokens[j];
				if (t.Value == "[")
				{
					++balance;
					ls.Add(t);
				}
				else if (t.Value == "]")
				{
					--balance;
					if (balance != 0)
						ls.Add(t);
				}
				else ls.Add(t);
			}

			var val = ExpressionParser.Parse(ls.ToArray(), script, substitutions);
			if (!colType.IndexType.Subsumes(val.Type.Type))
				throw new LsnrParsingException(tokens[index + 1], $"{leftExpr.Type.Name} must be indexed with a(n) {colType.IndexType.Name}, cannot use a(n) {val.Type.Name}", script.Path);
			var expr = new CollectionValueAccessExpression(leftExpr, val, colType.ContentsType.Id);
			return (expr, j + 1, 1);
		}
	}
}
