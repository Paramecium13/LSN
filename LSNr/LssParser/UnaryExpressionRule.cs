using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	public class UnaryExpressionRule : IExpressionRule
	{
		public uint Priority { get; }
		private readonly string OperatorValue;
		private readonly Func<IExpression, IExpression> CreateExpr;
		private readonly ContextCheck ContextCheck;

		public UnaryExpressionRule(string op, uint priority, Func<IExpression, IExpression> create,
			ContextCheck contextCheck = null)
		{
			OperatorValue = op; Priority = priority; CreateExpr = create; ContextCheck = contextCheck;
		}

		public bool CheckToken(Token token, IPreScript script)
			=> token.Value == OperatorValue;

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> ContextCheck != null ? ContextCheck(index, tokens, script, substitutions) : true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var expr = substitutions[tokens[index + 1]];
			return (CreateExpr(expr), index + 2, 0);
		}

		public static readonly IExpressionRule NotRule = new UnaryExpressionRule("!", ExpressionRulePriorities.Logical, (e) => new NotExpression(e));

		public static readonly IExpressionRule NegationRule = new UnaryExpressionRule("-", ExpressionRulePriorities.Negation,
			(e) => new BinaryExpression(new LsnValue(-1), e, BinaryOperation.Product, BinaryExpression.GetArgTypes(LsnType.int_.Id, e.Type)),
			(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)=>
			{
				if (index == 0)
					return true;
				var left = tokens[index - 1];
				switch (left.Type)
				{
					case TokenType.Float:
					case TokenType.Identifier:
					case TokenType.Substitution:
					case TokenType.Integer:
						return false;
					case TokenType.Keyword:
					case TokenType.Operator:
					case TokenType.SyntaxSymbol:
						return true;
					default:
						return false;
				}
			});
	}
}
