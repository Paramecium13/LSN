using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Expressions
{
	[Serializable]
	public class VariableExpression : Expression
	{
		public string Name { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public VariableExpression(string name, TypeId type)
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

		public VariableExpressionB(int index, TypeId type)
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
