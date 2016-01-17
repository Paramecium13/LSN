using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core.Expressions
{
	public class FunctionCall : ComponentExpression
	{
		private Function Fn;

		private string FnName;

		private Dictionary<string, IExpression> Args;

		public FunctionCall(Function fn, Dictionary<string,IExpression> args, bool include = false)
		{
			if (include) Fn = fn;
			else FnName = fn.Name;
			Args = args;
		}


		public override ILSN_Value Eval(IInterpreter i)
		{
			var fn = Fn ?? i.GetFunction(FnName);
			i.EnterFunctionScope(fn.Environment);
			var val = fn.Eval(Args, i);
			i.ExitFunctionScope();
			return val;
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

		public override string TranslateUniversal()
		{
			throw new NotImplementedException();
		}
	}
}
