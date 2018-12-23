using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Collections;

namespace LsnCore.Expressions
{
	public abstract class Expression : IExpression
	{
		private TypeId _Type;

		public /*virtual*/ TypeId Type { get { return _Type; } protected set { _Type = value; } }

		public abstract bool IsPure { get; }

		public abstract bool IsReifyTimeConst();

		public abstract IExpression Fold();

		public abstract LsnValue Eval(IInterpreter i);
		public virtual void Replace(IExpression oldExpr, IExpression newExpr) { }
		public virtual bool Equals(IExpression other) => this == other;
		public abstract void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer);
		public abstract IEnumerator<IExpression> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
