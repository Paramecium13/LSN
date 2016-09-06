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
		public IExpression Value;
		private readonly int Index;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public FieldAccessExpression(IExpression value, string name, LsnType type)
		{
			Value = value;
			Index = ((IHasFieldsType)value.Type.Type).GetIndex(name);
			Type = type.Id;
		}

		public override bool IsReifyTimeConst()
			=> Value.IsReifyTimeConst();

		public override IExpression Fold()
		{
			Value = Value.Fold();
			var hasFields = Value as IHasFieldsValue;
			if (hasFields != null)
				return hasFields.GetValue(Index);
			return this;
		}

		public override ILsnValue Eval(IInterpreter i)
			=> ((IHasFieldsValue)Value).GetValue(Index);

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
