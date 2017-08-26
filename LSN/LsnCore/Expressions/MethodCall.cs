using LsnCore.Expressions;
using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LsnCore.Expressions
{
	[Serializable]
	public class MethodCall : Expression
	{

		private IExpression _Value;

		/// <summary>
		/// The object that is calling this method.
		/// </summary>
		public IExpression Value { get { return _Value; } set { _Value = value; } }
		public IExpression[] Args;

		public override bool IsPure => false;

		public readonly Method Method;
		private readonly string MethodName;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MethodCall(Method m, /*IExpression value,*/ IExpression[] args)
		{
			//Value = value;
			Args = args ?? new IExpression[1];
			Method = m;
			Type = m.ReturnType;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MethodCall(string name, /*IExpression value,*/ TypeId type, IExpression[] args )
		{
			//Value = value;
			Args = args;
			MethodName = name;
			Type = type;
		}

		public override LsnValue Eval(IInterpreter i)
		{
			var args = new LsnValue[Args.Length];
			for (int x = 0; x < Args.Length; x++)
				args[x] = Args[x].Eval(i);
			var fn = Method ?? args[0].Type.Type.Methods[MethodName]; //TODO:...
			if (!fn.HandlesScope) i.EnterFunctionScope(fn.ResourceFilePath, fn.StackSize);
			var val = fn.Eval(args, i);
			if (!fn.HandlesScope) i.ExitFunctionScope();
			return val;
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

	}
}
