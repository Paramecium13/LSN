using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;

namespace LsnCore.Expressions
{
	public sealed class NotExpression : IExpression
	{
		internal IExpression Value;

		public TypeId Type => LsnType.Bool_.Id;

		public bool IsPure => Value.IsPure;

		internal NotExpression(IExpression value)
		{
			Value = value;
		}

		public bool Equals(IExpression other)
		{
			var n = other as NotExpression;
			if (n == null) return false;
			return Value.Equals(n.Value);
		}

		public LsnValue Eval(IInterpreter i)
		{
			return LsnBoolValue.GetBoolValue(!Value.Eval(i).BoolValue);
		}

		public IExpression Fold()
		{
			Value = Value.Fold();
			var cnst = Value as LsnValue?;
			if(cnst.HasValue)
			{
				var c = cnst.Value;
				return LsnBoolValue.GetBoolValue(!c.BoolValue);
			}
			var n = Value as NotExpression;
			if(n != null)
				return n.Value;
			return this;
		}

		public bool IsReifyTimeConst()
			=> Value.IsReifyTimeConst();

		public void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr))
				Value = newExpr;
			else
				Value.Replace(oldExpr, newExpr);
		}
	}
}
