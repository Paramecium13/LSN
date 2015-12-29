using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	public class MethodCall : Expression
	{

		private IExpression _Value;
		public IExpression Value { get { return _Value; } set { _Value = value; } }
		private Dictionary<string, IExpression> Args;

		private Method M;


		public MethodCall(Method m, Dictionary<string, IExpression> args, IExpression value)
		{
			Value = value;
			Args = args;
			M = m;
			Args.Add("self", Value);
		}

		public override ILSN_Value Eval(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

	}
}
