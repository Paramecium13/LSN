using LsnCore.Expressions;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// An object representing an LSN value.
	/// </summary>
	[Serializable]
	public abstract class LsnValueB : Expression, ILsnValue
	{
		public abstract bool BoolValue { get; }

		public abstract ILsnValue Clone();

		public override bool IsPure => true;

		public override LsnValue Eval(IInterpreter i)
		{
			return new LsnValue(this);//Clone() ?
		}

		public override IExpression Fold()
		{
			return this;
		}

		public override bool IsReifyTimeConst() => true;

	}
}
