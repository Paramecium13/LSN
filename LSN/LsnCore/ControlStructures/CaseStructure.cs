using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.ControlStructures
{

	[Serializable]
	public class CaseStructure : ControlStructure
	{
		private IExpression _Value;
		internal IExpression Value { get { return _Value; } private set { _Value = value; } }
		private readonly IReadOnlyList<Component> Components;

		public CaseStructure(IExpression value, List<Component> components)
		{
			Value = value; Components = components;
		}

		public override InterpretValue Interpret(IInterpreter i)
			=> Interpret(Components, i);
	}
}
