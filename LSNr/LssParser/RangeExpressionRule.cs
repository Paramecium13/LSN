using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Utilities;

namespace LSNr.LssParser
{
	public sealed class RangeExpressionRule : IExpressionRule
	{
		internal static IExpressionRule Rule = new RangeExpressionRule();

		public uint Priority => ExpressionRulePriorities.Range;

		private RangeExpressionRule() { }

		public bool CheckToken(Token token, IPreScript script)
			=> token.Type == TokenType.Operator && token.Value == "..";

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> tokens[index].Type == TokenType.Operator && tokens[index].Value == "..";

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script,
			IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var left = Create.Express(tokens.ToSlice().CreateSliceSkipTake(index - 1, 1), script, substitutions);
			var right = Create.Express(tokens.ToSlice().CreateSliceSkipTake(index + 1, 1), script, substitutions);

			return ( new RangeExpression(left, right), index + 2, 1);
		}
	}
}
