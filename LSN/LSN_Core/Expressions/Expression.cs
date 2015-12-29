using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSN_Core.Compile;

namespace LSN_Core.Expressions
{
	[Serializable]
	public abstract class Expression : IExpression
	{
		public virtual LSN_Type Type { get; protected set; }

		public abstract bool IsReifyTimeConst();

		public abstract IExpression Fold();

		public abstract ILSN_Value Eval(IInterpreter i);

	}
}
