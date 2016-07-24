using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Expressions
{
	[Serializable]
	public class VariableExpression : Expression
	{
		public string Name { get; private set; }

		public VariableExpression(string name, LsnType type)
		{
			Name = name;
			Type = type;
		}

		public override ILsnValue Eval(IInterpreter i) => i.GetValue(Name);

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

	}

	[Serializable]
	public class VariableExpressionB : Expression
	{
		public int Index;

		public VariableExpressionB(int index, LsnType type)
		{
			Index = index; Type = type;
		}
		

		public override ILsnValue Eval(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;
		
	}
}
