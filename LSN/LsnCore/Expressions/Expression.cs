using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public abstract class Expression : IExpression
	{
		private LsnType _Type = LsnType.dynamic_;

		public virtual LsnType Type { get { return _Type; } protected set { _Type = value; } }

		public abstract bool IsReifyTimeConst();

		public abstract IExpression Fold();

		public abstract ILsnValue Eval(IInterpreter i);

	}
}
