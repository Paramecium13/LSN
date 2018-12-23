using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.ControlStructures
{
	/// <summary>
	/// A(n) LSN while loop...
	/// </summary>
	public class WhileLoop : ControlStructure
	{
		public IExpression Condition;
		public readonly List<Component> Body;

		public WhileLoop(IExpression condition, List<Component> body)
		{
			Condition = condition; Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new InvalidOperationException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}
}
