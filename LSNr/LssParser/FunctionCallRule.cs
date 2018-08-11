using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	public class FunctionCallRule : IExpressionRule
	{
		public uint Priority => ExpressionRulePriorities.Function;

		public bool CheckToken(Token token, IPreScript script)
			=> token.Type == TokenType.Identifier && script.CheckSymbol(token.Value) == SymbolType.Function;

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			throw new NotImplementedException();
		}
	}
}
