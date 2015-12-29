using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core.Expressions
{
	[Serializable]
	public class VariableExpression : ComponentExpression
	{
		public string Name { get; private set; }

		public VariableExpression(string name)
		{
			Name = name;
		}

		public override ILSN_Value Eval(IInterpreter i) => i.GetValue(Name);

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst()
		{
			throw new NotImplementedException();
		}

		public override string TranslateUniversal() => Name;
	}
}
