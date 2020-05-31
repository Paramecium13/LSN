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
			// ToDo: Support for positional arguments.
			var (args, nextIndex) = Create.CreateArgs(i, tokens, script, substitutions);

			switch (type)
			{
				case StructType structType:
					// ToDo: Check argument type and number!!!
					return (new StructConstructor(type.Id, args), nextIndex, 0);
				case RecordType recordType:
					// ToDo: Check argument type and number!!!
					return (new RecordConstructor(type.Id, args), nextIndex, 0);
				case LsnListType lsTy:
					return (new ListConstructor(lsTy), nextIndex, 0);
				default:
					throw new ApplicationException();
			}
		}
	}
}
