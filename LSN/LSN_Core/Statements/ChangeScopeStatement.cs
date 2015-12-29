using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core.Statements
{
	[Serializable]
	public class EnterScopeStatement : Statement
	{
		public override bool Interpret(IInterpreter i)
		{
			i.EnterScope();
			return true;
		}
	}
	[Serializable]
	public class ExitScopeStatement : Statement
	{
		public override bool Interpret(IInterpreter i)
		{
			i.ExitScope();
			return true;
		}
	}
}
