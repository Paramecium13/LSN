using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Statements
{
	/// <summary>
	/// Exit the innermost loop.
	/// </summary>
	[Serializable]
	public class BreakStatement : Statement
	{
		public override InterpretValue Interpret(IInterpreter i) => InterpretValue.Break;
	}

	/// <summary>
	/// Move on to the next iteration of the innermost loop.
	/// </summary>
	[Serializable]
	public class NextStatement : Statement
	{
		public override InterpretValue Interpret(IInterpreter i) => InterpretValue.Next;
	}

}
