using System;
using System.Collections.Generic;
using System.Linq;

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
			FnName = fn.Name;
			Args = args;
		}

		private FunctionCall(string fnName, Dictionary<string,IExpression> args)
		{
			FnName = fnName;
			Args = args;
		}

		public override ILSN_Value Eval(IInterpreter i)
		{
			var args = Args.Select(p => new KeyValuePair<string, ILSN_Value>(p.Key, p.Value.Eval(i))).ToDictionary();
            var fn = Fn ?? i.GetFunction(FnName);
			if (! fn.HandlesScope) i.EnterFunctionScope(fn.Environment);
			var val = fn.Eval(args, i);
			if (! fn.HandlesScope) i.ExitFunctionScope();
			return val;
		}

		public override IExpression Fold()
		{
			var dict = Args.Select(p => new KeyValuePair<string, IExpression>(p.Key, p.Value.Fold())).ToDictionary();
			if (Fn != null) return new FunctionCall(Fn, dict, true);
			return new FunctionCall(FnName,dict);
		}

		public override bool IsReifyTimeConst() => false;

		public override string TranslateUniversal()
		{
			throw new NotImplementedException();
		}
	}
}
