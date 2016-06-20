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
		private IExpression Condition;
		private List<Component> Body;

		public WhileLoop(IExpression condition, List<Component> body)
		{
			Condition = condition; Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.EnterScope();
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
						i.ExitScope();
						return InterpretValue.Base;
					case InterpretValue.Return:
						i.ExitScope();
						return InterpretValue.Return;
					default:
						break;
				}
			}
			i.ExitScope();
			return InterpretValue.Base;
		}
	}
}
