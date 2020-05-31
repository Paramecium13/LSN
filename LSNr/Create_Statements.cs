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
	internal static partial class Create
	{

		/// <summary>
		/// Creates a <see cref="Statement"/> from the provided <see cref="Token"/>s.
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

			//throw new LsnrParsingException(tokens[0], "Could not parse statement.", script.Path);
		}
	}
}