using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Statements;
using LsnCore.Utilities;

namespace LSNr.Statements
{
	public class SetStateStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t)
			=> t.Type == TokenType.Keyword && (t.Value == "set" || t.Value == "setstate");

		public bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens[0].Value == "setstate" || tokens.TestAt(1, t => t.Value == "state");

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			var offset = tokens[0].Value == "setstate" ? 0 : 1;
			if (tokens[offset + 1].Value == "to") ++offset;
			var stateName = tokens[offset].Value;
			var pre = script as PreScriptClassFunction;
			if (!pre.Parent.StateExists(stateName))
				throw new LsnrParsingException(tokens[offset], $"The script class '{pre.Parent.Id.Name}' does not have a state '{stateName}'.", script.Path);
			return new SetStateStatement(pre.Parent.GetStateIndex(stateName));
		}
	}
}