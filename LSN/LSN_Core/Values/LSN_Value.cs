using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// An object representing an LSN value.
	/// </summary>
	[Serializable]
	public abstract class LSN_Value : ComponentExpression, ILSN_Value
	{

		public abstract ILSN_Value Clone();

		public override ILSN_Value Eval(IInterpreter i)
		{
			return this;//Clone() ?
		}

		public override IExpression Fold()
		{
			return this;
		}

		public abstract int GetSize();
		public override bool IsReifyTimeConst() => true;

	}
}
