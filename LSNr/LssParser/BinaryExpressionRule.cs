using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	public delegate bool ContextCheck(int index, Token[] tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions);

	public class BinaryExpressionRule : IRule
	{
		public uint Priority { get; private set; }
		private readonly string operatorValue;
		private readonly Func<IExpression, IExpression, IExpression> createExpr;
		private readonly ContextCheck contextCheck;

		public BinaryExpressionRule(uint priority, string opVal, Func<IExpression, IExpression, IExpression> create, ContextCheck cxtCheck = null)
		{
			Priority = priority; operatorValue = opVal; createExpr = create; contextCheck = cxtCheck;
			if (create == null)
				throw new ArgumentNullException(nameof(create));
		}

		public bool CheckToken(Token token, IPreScript script)
			=> token.Value == operatorValue;

		public bool CheckContext(int index, Token[] tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> contextCheck?.Invoke(index, tokens, script, substitutions) ?? true;

			public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, Token[] tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			IExpression left, right;
			if (index == 0) throw new ApplicationException();

			left = null;
			right = null;

			if (substitutions.ContainsKey(tokens[index - 1]))
				left = substitutions[tokens[index - 1]];
			else left = Create.SingleTokenExpress(tokens[index - 1], script);

			if (substitutions.ContainsKey(tokens[index + 1]))
				right = substitutions[tokens[index - 1]];
			else right = Create.SingleTokenExpress(tokens[index + 1], script);

			return (createExpr(left, right), index + 2, 1);
		}

	}
}
