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
	public class ConstructorRule : IExpressionRule
	{
		public uint Priority => ExpressionRulePriorities.MemberAccess;

		public bool CheckToken(Token token, IPreScript script)
			=> token.Type == TokenType.Keyword && token.Value == "new";

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var i = index + 1;
			var type = script.ParseType(tokens, i, out i);

			var x = Create.CreateArgs(i, tokens, script, substitutions);

			if (type is StructType structType)
			{
				// ToDo: Check argument type and number!!!
				return (new StructConstructor(type.Id, x.args), x.nextIndex, 0);
			}
			if (type is RecordType recordType)
			{
				// ToDo: Check argument type and number!!!
				return (new RecordConstructor(type.Id, x.args), x.nextIndex, 0);
			}

			if (type is LsnListType lsTy)
				return (new ListConstructor(lsTy), x.nextIndex, 0);
			throw new ApplicationException();
		}
	}
}
