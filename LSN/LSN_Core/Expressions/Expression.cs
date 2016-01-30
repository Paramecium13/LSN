using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	[Serializable]
	public abstract class Expression : IExpression
	{
		private LSN_Type _Type = LSN_Type.dynamic_;

		public virtual LSN_Type Type { get { return _Type; } protected set { _Type = value; } }

		public abstract bool IsReifyTimeConst();

		public abstract IExpression Fold();

		public abstract ILSN_Value Eval(IInterpreter i);

	}
}
