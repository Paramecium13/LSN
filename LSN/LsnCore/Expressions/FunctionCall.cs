using System;
using System.Collections.Generic;
using System.Linq;

namespace LsnCore.Expressions
{
	[Serializable]
	public class FunctionCall : Expression
	{
		private Function Fn;

		private string FnName;

		private Dictionary<string, IExpression> Args;

		public FunctionCall(Function fn, Dictionary<string,IExpression> args, bool include = false)
		{
			if (include) Fn = fn;
			FnName = fn.Name;
			Args = args;
			Type = fn.ReturnType;
		}

		private FunctionCall(string fnName, Dictionary<string,IExpression> args, LsnType type = null)
		{
			FnName = fnName;
			Args = args;
			Type = type;
		}

		public override ILsnValue Eval(IInterpreter i)
		{
			var args = Args.Select(p => new KeyValuePair<string, ILsnValue>(p.Key, p.Value.Eval(i))).ToDictionary();
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

	}
}
