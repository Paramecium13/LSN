using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.ControlStructures
{
	public class Choice : ControlStructure
	{
		private List<Component> _Components;
		public List<Component> Components { get { return _Components; } set { _Components = value; } }
		public readonly IExpression Title;
		private IExpression Condition;

		public Choice(IExpression title, List<Component> components, IExpression condition = null)
		{
			Components = components; Title = title; Condition = condition;
		}

		public override InterpretValue Interpret(IInterpreter i)
			=> Interpret(_Components, i);


		public bool Check(IInterpreter i) => Condition?.Eval(i)?.BoolValue ?? true;

	}
}
