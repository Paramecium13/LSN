using LSN_Core.Compile;
using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core.Statements
{
	/// <summary>
	/// Assigns a new value to a variable.
	/// </summary>
	[Serializable]
	public class ReassignmentStatement : Statement
	{
		protected string Name { get; set; }

		protected IExpression Value { get; set; }

		public ReassignmentStatement(List<IToken> tokens)
		{
			Name = tokens[0].Value;
		}

		public override bool Interpret(IInterpreter i)
		{
			i.ReAssignVariable(Name, Value.Eval(i));
			return true;
		}

		public void Optimise()
		{
			Value = Value.Fold();
		}

	}
}
