using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// An object representing an LSN value.
	/// </summary>
	[Serializable]
	public abstract class LsnValue : Expression, ILsnValue
	{
		public abstract bool BoolValue { get; }

		public abstract ILsnValue Clone();

		public override ILsnValue Eval(IInterpreter i)
		{
			return this;//Clone() ?
		}

		public override IExpression Fold()
		{
			return this;
		}

		public override bool IsReifyTimeConst() => true;

	}
}
