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
		private readonly int Index;

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

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr)) Value = newExpr;
		}

		public override bool Equals(IExpression other)
		{
			var e = other as FieldAccessExpression;
			if (e == null) return false;
			return Value.Equals(e.Value);
		}
	}
}
