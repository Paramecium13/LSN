using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.ControlStructures
{
	public class CaseStructure : ControlStructure
	{
		public IExpression Value { get; private set; }

		public readonly IReadOnlyList<Component> Components;

		public CaseStructure(IExpression value, IReadOnlyList<Component> components)
		{
			Value = value; Components = components;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (oldExpr.Equals(Value)) Value = newExpr;
		}
	}
}
