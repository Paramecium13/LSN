using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	public class ValueExpression : Expression
	{

		public readonly ILSN_Value Value;

		public ValueExpression(ILSN_Value value)
		{
			Value = value;
			Type = value.Type;
		}

		public override ILSN_Value Eval(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => /* **** Yes! */ true;
	}
}
