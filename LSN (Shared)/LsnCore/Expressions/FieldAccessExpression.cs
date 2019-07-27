using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class FieldAccessExpression : Expression
	{
		public IExpression Value;
		internal readonly int Index;

		public override bool IsPure => true;

		internal FieldAccessExpression(IExpression fieldOwner, Field field)
		{
			Value = fieldOwner; Index = field.Index; Type = field.Type;
		}

		public FieldAccessExpression(IExpression value, int index)
		{
			Value = value;
			Index = index;
		}

		public override bool IsReifyTimeConst()
			=> Value.IsReifyTimeConst();

		public override IExpression Fold()
		{
			Value = Value.Fold();
			var hasFields = Value as IHasFieldsValue;
			if (hasFields != null)
				return hasFields.GetFieldValue(Index);
			return this;
		}

#if CORE
		public override LsnValue Eval(IInterpreter i)
			=> ((IHasFieldsValue)Value.Eval(i).Value).GetFieldValue(Index);
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr)) Value = newExpr;
		}

		public override bool Equals(IExpression other)
		{
			var e = other as FieldAccessExpression;
			if (e == null) return false;
			return Index == e.Index && Value.Equals(e.Value);
		}

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.FieldAccess);
			writer.Write((ushort)Index);
			Value.Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}
	}
}
