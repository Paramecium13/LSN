using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public class FieldAccessExpression : Expression
	{
		private IExpression Value;
		private readonly string FieldName;

		public FieldAccessExpression(IExpression value, string name, LsnType type)
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

		public override ILsnValue Eval(IInterpreter i)
			=> ((IHasFieldsValue)Value).GetValue(FieldName);
    }
}
