using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	public delegate bool ContextCheck(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions);

	public class BinaryExpressionRule : IExpressionRule
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

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> contextCheck?.Invoke(index, tokens, script, substitutions) ?? true;

			public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
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

		public static readonly IExpressionRule Sum = new BinaryExpressionRule(ExpressionRulePriorities.AddSub, "+",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.Sum, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule Difference = new BinaryExpressionRule(ExpressionRulePriorities.AddSub, "-",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.Difference, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule Product = new BinaryExpressionRule(ExpressionRulePriorities.MultDiv, "*",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.Product, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule Quotient = new BinaryExpressionRule(ExpressionRulePriorities.MultDiv, "/",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.Quotient, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule Modulus = new BinaryExpressionRule(ExpressionRulePriorities.MultDiv, "%",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.Modulus, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule LessThan = new BinaryExpressionRule(ExpressionRulePriorities.Comparative, "<",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.LessThan, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule LessThanOrEqual = new BinaryExpressionRule(ExpressionRulePriorities.Comparative, "<=",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.LessThanOrEqual, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule GreaterThan = new BinaryExpressionRule(ExpressionRulePriorities.Comparative, ">",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.GreaterThan, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule GreaterThanOrEqual = new BinaryExpressionRule(ExpressionRulePriorities.Comparative, ">=",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.GreaterThanOrEqual, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule Equal = new BinaryExpressionRule(ExpressionRulePriorities.Comparative, "==",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.Equal, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule NotEqual = new BinaryExpressionRule(ExpressionRulePriorities.Comparative, "!=",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.NotEqual, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule LogicalAnd = new BinaryExpressionRule(ExpressionRulePriorities.Comparative, "&&",
			(l, r) => new BinaryExpression(l, r, BinaryOperation.And, BinaryExpression.GetArgTypes(l.Type, r.Type)));

		public static readonly IExpressionRule LogicalOr = new BinaryExpressionRule(ExpressionRulePriorities.Comparative, "||",
					(l, r) => new BinaryExpression(l, r, BinaryOperation.Or, BinaryExpression.GetArgTypes(l.Type, r.Type)));
	}
}
