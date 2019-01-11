using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSNr
{
	/// <summary>
	/// ...
	/// </summary>
	internal static partial class Create
	{

		/// <summary>
		/// Creates a statement.
		/// </summary>
		/// <param name="tokens"> The tokens of the statement, not including the ';'.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static Statement State(ISlice<Token> tokens, IPreScript script)
		{
			var first = tokens[0];
			foreach (var rule in script.StatementRules)
			{
				if (rule.PreCheck(first) && rule.Check(tokens, script))
					return rule.Apply(tokens, script);
			}
			// Expression statement:
			// When all else fails, parse the whole thing as an expression.
			return new ExpressionStatement(Express(tokens, script));
			// The top level expression should be a function call, method call, ScriptObjectMethodCall, or HostInterfaceMethodCall.
			// If it isn't, complain.

			throw new LsnrParsingException(tokens[0], "Could not parse statement.", script.Path);
		}

		/// <summary>
		/// Used in making give statements.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="str"></param>
		/// <param name="indexOfString"></param>
		/// <param name="script"></param>
		/// <returns></returns>
		private static IExpression GetExpression(IEnumerable<Token> tokens, string str, out int indexOfString, IPreScript script)
		{
			indexOfString = tokens.Select(t => t.Value.ToLower()).ToList().IndexOf(str);
			var exprTokens = tokens.Take(indexOfString - 1).ToList();
			return Express(exprTokens, script);
		}

		private static GiveGoldStatement GiveGold(ISlice<Token> tokens, IPreScript script)
		{
			// ToDo: Change to like GiveItem(...).
			IExpression Amount;
			IExpression receiver = LsnValue.Nil;

			int indexOfKeywordGold = tokens.Select(t => t.Value).ToList().IndexOf("gold");
			if(tokens.Any(t => t.Value == "to"))
			{
				int i;
				Amount = GetExpression(tokens, "to", out i, script);
				receiver = Express(tokens.Skip(i + 1), script);
			}
			else
				Amount = Express(tokens.Skip(1).Take(indexOfKeywordGold - 1).ToList(), script);
			return new GiveGoldStatement(Amount,receiver);
		}
	}
}