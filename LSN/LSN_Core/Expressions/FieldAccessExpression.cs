using LSN_Core.Types;
using LSN_Core.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	public class FieldAccessExpression : Expression
	{
		private IExpression Value;
		private readonly string FieldName;

		public FieldAccessExpression(IExpression value, string name, LSN_Type type)
		{
			Value = value;
			FieldName = name;
			Type = type;
		}

		public override bool IsReifyTimeConst()
			=> Value.IsReifyTimeConst();

		public override IExpression Fold()
		{
			Value = Value.Fold();
			return this;
		}

		public override ILSN_Value Eval(IInterpreter i)
			=> ((IHasFieldsValue)Value).GetValue(FieldName);
    }
}
