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
			while(Condition.Eval(i).BoolValue)
			{
				var val = Interpret(Body, i);
				switch (val)
				{
					case InterpretValue.Base:
						break;
					case InterpretValue.Next:
						break;
					case InterpretValue.Break:
						return InterpretValue.Base;
					case InterpretValue.Return:
						return InterpretValue.Return;
					default:
						break;
				}
			}
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}
}
