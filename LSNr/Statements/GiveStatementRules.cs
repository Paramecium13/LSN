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
	public sealed class GiveItemStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.GiveItem;

		public bool PreCheck(Token t) => t.Value == "give";

		public bool Check(ISlice<Token> tokens, IPreScript script)
			=> true;

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			//		give [amount] [item] id [to receiver];
			// (a)	give 10 item cat [to bob];
			// (b)	give item cat [to bob];
			// (c)	give cat [to bob];
			// (d)	give 10 cat [to bob]
			IExpression amount; var amountIndex = 0;
			IExpression id; int idIndex;
			IExpression receiver = LsnValue.Nil;

			var res = LssParser.ExpressionParser.MultiParse(tokens, script);
			var len = res.tokens.Length;
			var expTokens = Slice<Token>.Create(res.tokens, 0, len);
			if (expTokens[1].Type == TokenType.Substitution)
			{ // It's not type (b)
				if (len == 2 || len > 2 && expTokens[2].Value == "to")
					idIndex = 1; // It's type (c)
				else
				{ // It's type (a) or (d)
					amountIndex = 1;
					if (expTokens[2].Value == "item")
						idIndex = 3; // It's type (a)
					else idIndex = 2; // It's type (d)
				}
			}
			else idIndex = 2; // It's type (b)

			if (amountIndex != 0)
			{
				if (expTokens[amountIndex].Type != TokenType.Substitution)
					throw new LsnrParsingException(expTokens[1], "Improperly formatted give item statement.", script.Path);
				amount = res.substitutions[expTokens[amountIndex]];
				if (amount.Type != LsnType.int_.Id)
					throw new LsnrParsingException(tokens[0], "Improperly formatted give item statement: amount must be an int expression.", script.Path);
			}
			else amount = new LsnValue(1);

			if (expTokens[idIndex].Type != TokenType.Substitution)
				throw new LsnrParsingException(expTokens[1], "Improperly formatted give item statement.", script.Path);
			id = res.substitutions[expTokens[idIndex]];

			var toIndex = idIndex + 1;
			if (len > toIndex)
			{
				if (expTokens[toIndex].Value != "to")
				{
					if (expTokens[toIndex].Type == TokenType.Substitution)
						throw new LsnrParsingException(tokens[0], "Improperly formatted give item statement: expected 'to' or ';' received expression.", script.Path);
					throw LsnrParsingException.UnexpectedToken(expTokens[toIndex], "'to' or ';'", script.Path);
				}
				if (len == toIndex + 1)
					throw new LsnrParsingException(expTokens[toIndex], "Improperly formatted give item statement: unexpected end of statement.", script.Path);
				if (len != toIndex + 2)
				{
					if (expTokens[toIndex + 2].Type == TokenType.Substitution)
						throw new LsnrParsingException(tokens[0], "Improperly formatted give item statement: expected ';' received expression.", script.Path);
					throw LsnrParsingException.UnexpectedToken(expTokens[toIndex + 2], ";", script.Path);
				}
				if (expTokens[toIndex + 1].Type != TokenType.Substitution)
					throw LsnrParsingException.UnexpectedToken(expTokens[toIndex + 1], "an expression", script.Path);
				receiver = res.substitutions[expTokens[toIndex + 1]];
			}
			return new GiveItemStatement(id, amount, receiver);
		}
	}
}
