using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	internal class RangeExpression : IExpression
	{
#if LSNR
		public
#else
		private readonly
#endif
		IExpression Start;

#if LSNR
		public
#else
		private readonly
#endif
		IExpression End;

		public RangeExpression(IExpression start, IExpression end)
		{
			Start = start;
			End = end;
		}

		public TypeId Type => RangeType.Instance.Id;

		public bool IsPure => false;

		public bool Equals(IExpression other)
		{
			var o = other as RangeExpression;
			if (o == null) return false;
			return o.Start.Equals(Start) && o.End.Equals(End);
		}

#if CORE
		public LsnValue Eval(IInterpreter i) => new LsnValue(new RangeValue(Start.Eval(i).IntValue, End.Eval(i).IntValue));
#endif

		public IExpression Fold()
		{
#if LSNR
			Start = Start.Fold();
			End = End.Fold();
			if (Start is LsnValue s && End is LsnValue e)
				return new LsnValue(new RangeValue(s.IntValue, e.IntValue));
			return this;
#else
			throw new InvalidOperationException();
#endif
		}

		public IEnumerator<IExpression> GetEnumerator()
		{
			yield return Start;
			foreach (var expr in Start.SelectMany(e => e))
				yield return expr;
			yield return End;
			foreach (var expr in End.SelectMany(e => e))
				yield return expr;
		}

		public bool IsReifyTimeConst() => Start.IsReifyTimeConst() && End.IsReifyTimeConst();

		public void Replace(IExpression oldExpr, IExpression newExpr)
		{
#if LSNR
			if (Start.Equals(oldExpr)) Start = newExpr;
			else Start.Replace(oldExpr, newExpr);
			if (End.Equals(oldExpr)) End = newExpr;
			else End.Replace(oldExpr, newExpr);
#endif
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.RangeExpression);
			Start.Serialize(writer, resourceSerializer);
			End.Serialize(writer, resourceSerializer);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
