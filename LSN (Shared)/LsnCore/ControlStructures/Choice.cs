using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.ControlStructures
{
	public class Choice : ControlStructure
	{
		public IList<Component> Components { get; }
		public IExpression Title { get; private set; }
		public IExpression Condition;

		public Choice(IExpression title, List<Component> components, IExpression condition = null)
		{
			Components = components; Title = title; Condition = condition;
		}

		public override InterpretValue Interpret(IInterpreter i)
			=> throw new NotImplementedException();

		public bool Check(IInterpreter i) => Condition?.Eval(i).BoolValue ?? true;

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Title.Equals(oldExpr)) Title = newExpr;
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}
}
