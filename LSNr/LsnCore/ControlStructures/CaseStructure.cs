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
		private IExpression _Value;
		public IExpression Value { get { return _Value; } private set { _Value = value; } }
		public readonly IReadOnlyList<Component> Components;

		public CaseStructure(IExpression value, List<Component> components)
		{
			Value = value; Components = components;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (oldExpr.Equals(Value)) Value = newExpr;
		}
	}
}
