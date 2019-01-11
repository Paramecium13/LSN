using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Statements;
using LsnCore.Utilities;

namespace LSNr.Statements
{
	public class BreakStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t)
			=> t.Value == "break";

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
			=> new BreakStatement();

		public bool Check(ISlice<Token> tokens, IPreScript script) => true;
	}

	public class NextStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t)
			=> t.Value == "next";

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
			=> new NextStatement();

		public bool Check(ISlice<Token> tokens, IPreScript script) => true;
	}

	public class ReturnStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t)
			=> t.Value == "return";

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			if (tokens.Length > 1)
				return new ReturnStatement(Create.Express(tokens.Skip(1).ToList(), script));
			return new ReturnStatement(null);
		}

		public bool Check(ISlice<Token> tokens, IPreScript script) => true;
	}
}
