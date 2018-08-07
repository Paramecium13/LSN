using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	public class ConstantRule : IExpressionRule
	{
		public static readonly IExpressionRule Rule = new ConstantRule();

		public uint Priority => ExpressionRulePriorities.Constant;

		private ConstantRule() { }

		public bool CheckToken(Token token, IPreScript script)
		{
			switch (token.Type)
			{
				case TokenType.Float:
				case TokenType.Integer:
				case TokenType.String:
					return true;
				default:
					return false;
			}
		}

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft) CreateExpression(int index, IReadOnlyList<Token> tokens,
			IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var token = tokens[index];
			switch (token.Type)
			{
				case TokenType.Float:
					return (new LsnValue(token.DoubleValue), index + 1, 0);
				case TokenType.Integer:
					return (new LsnValue(token.IntValue), index + 1, 0);
				case TokenType.String:
					return (new LsnValue(new StringValue(token.Value)), index + 1, 0);
				default:
					throw new ApplicationException();
			}
		}
	}
}
