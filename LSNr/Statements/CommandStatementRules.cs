using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Utilities;

namespace LSNr.Statements
{
	public sealed class SayStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t) => t.Value == "say";

		public bool Check(ISlice<Token> tokens, IPreScript script) => true;

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			tokens = tokens.CreateSliceAt(1);
			IExpression message, graphic = LsnValue.Nil, title = LsnValue.Nil;
			var asIndex = tokens.IndexOf("as");
			var withIndex = tokens.IndexOf("with");
			if (withIndex < 0) withIndex = tokens.IndexOf("withgraphic");
			if (withIndex > 0)
			{
				if (asIndex > 0)
				{
					var firstIndex = Math.Min(withIndex, asIndex);
					var secondIndex = Math.Max(withIndex, asIndex);
					message = Create.Express(tokens.CreateSliceTaking(firstIndex), script);
					var expr2 = Create.Express(tokens.CreateSliceBetween(firstIndex, secondIndex), script);
					var expr3 = Create.Express(tokens.CreateSliceAt(secondIndex), script);
					if (firstIndex == withIndex) { graphic = expr2; title = expr3; }
					else { title = expr2; graphic = expr3; }
				}
				else
				{
					message = Create.Express(tokens.CreateSliceTaking(withIndex), script);
					graphic = Create.Express(tokens.CreateSliceAt(withIndex), script);
				}
			}
			else if (asIndex > 0)
			{
				message = Create.Express(tokens.CreateSliceTaking(asIndex), script);
				title = Create.Express(tokens.CreateSliceAt(asIndex), script);
			}
			else // No title or graphic
				message = Create.Express(tokens, script);

			return new SayStatement(message, graphic, title);
		}
	}
}
