using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core.Expressions
{
	public class FunctionCall : Expression
	{
		private Function Fn;

		private Dictionary<string, IExpression> Args;

		public FunctionCall(Function fn, Dictionary<string,IExpression> args)
		{
			Fn = fn;
			Args = args;
		}


		public override ILSN_Value Eval(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;
	}
}
