﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public abstract class Expression : IExpression
	{
		private LsnType _Type;

		public virtual LsnType Type { get { return _Type; } set { _Type = value; } }

		public abstract bool IsReifyTimeConst();

		public abstract IExpression Fold();

		public abstract ILsnValue Eval(IInterpreter i);
		public virtual void Replace(IExpression oldExpr, IExpression newExpr) { }
		public virtual bool Equals(IExpression other) => this == other;
	}
}
