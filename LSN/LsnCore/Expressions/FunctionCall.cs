using System;
using System.Collections.Generic;
using System.Linq;

namespace LsnCore.Expressions
{
	[Serializable]
	public class FunctionCall : Expression
	{
		public Function Fn;

		private string FnName;

		public override bool IsPure => false;

		//ToDo: Encapsulate? Used in ExpressionWalker...
		public readonly IExpression[] Args;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public FunctionCall(Function fn, IExpression[] args, bool include = false)
		{
			if (include) Fn = fn;
			FnName = fn.Name;
			Args = args;
			Type = fn.ReturnType;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		private FunctionCall(string fnName, IExpression[] args, LsnType type = null)
		{
			FnName = fnName;
			Args = args;
			Type = type?.Id;
		}

		public override LsnValue Eval(IInterpreter i)
		{
			var args = new LsnValue[Args.Length];
			for (int x = 0; x < Args.Length; x++)
				args[x] = Args[x].Eval(i);
			//var args = Args.Select(a => a.Eval(i)).ToArray();

			var fn = Fn ?? i.GetFunction(FnName);

			var val = fn.Eval(args, i);
			
			return val;
		}

		public override IExpression Fold()
		{
			var dict = Args.Select(a => a.Fold()).ToArray();
			if (Fn != null) return new FunctionCall(Fn, dict, true);
			return new FunctionCall(FnName,dict);
		}

		public override bool IsReifyTimeConst() => false;

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if(Args.Contains(oldExpr))
				Args[Array.IndexOf(Args, oldExpr)] = newExpr;
		}

		public override bool Equals(IExpression other)
		{
			var e = other as FunctionCall;
			if (e == null) return false;
			return e.Args.SequenceEqual(Args);
		}
	}
}
