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

	public sealed class GoToStatementRule : IStatementRule
	{
		public int Order => throw new NotImplementedException();

		public bool PreCheck(Token t) => true;

		public bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens.Any(t => t.Value == "goto");

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			/*var res = LssParser.ExpressionParser.MultiParse(tokens, script);
			tokens = res.tokens.ToSlice();
			var subs = res.substitutions;
			if(tokens[0].Type == TokenType.Substitution)
			...*/
			IExpression actor = null;
			IReadOnlyList<Token> tokens0 = null;
			if (tokens[0].Value == "goto")
				tokens0 = tokens;
			else
			{
				var actorTokens = tokens.TakeWhile(t => t.Value != "goto").ToList();
				tokens0 = tokens.Skip(actorTokens.Count + 1).ToList();
				actor = Create.Express(actorTokens, script);
			}

			var metaCommaCount = tokens0.Count(t => t.Value == "`");
			IExpression expr0 = null;
			IExpression expr1 = null;
			IExpression expr2 = null;
			switch (metaCommaCount)
			{
				case 0:
					expr0 = Create.Express(tokens0, script);
					break;
				case 1:
					{
						var tokens1 = tokens0.TakeWhile(t => t.Value != "`").ToList();
						var tokens2 = tokens0.Skip(tokens1.Count + 1).ToList();
						expr0 = Create.Express(tokens1, script);
						expr1 = Create.Express(tokens2, script);
						break;
					}
				case 2:
					{
						var tokens1 = tokens0.TakeWhile(t => t.Value != "`").ToList();
						var tokens2 = tokens0.Skip(tokens1.Count + 1).TakeWhile(t => t.Value != "`").ToList();
						var tokens3 = tokens0.Skip(tokens1.Count + tokens2.Count + 2).ToList();
						expr0 = Create.Express(tokens1, script);
						expr1 = Create.Express(tokens2, script);
						expr2 = Create.Express(tokens3, script);
						break;
					}
				default:
					throw new LsnrParsingException(tokens.First(t => t.Value.ToLower() == "goto"), "Improperly formatted goto statement (considered harmful).", script.Path);
			}
			return new GoToStatement(expr0, expr1, expr2, actor);
		}
	}
}
