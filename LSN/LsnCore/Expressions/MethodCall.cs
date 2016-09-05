using LsnCore.Expressions;
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
		private Dictionary<string, IExpression> Args;

		private readonly Method M;
		private readonly string MethodName;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MethodCall(Method m, IExpression value, Dictionary<string, IExpression> args = null)
		{
			Value = value;
			Args = args ?? new Dictionary<string, IExpression>();
			M = m;
			Args.Add("self", Value);
			Type = m.ReturnType;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MethodCall(string name, IExpression value, LsnType type, Dictionary<string, IExpression> args = null)
		{
			Value = value;
			Args = args ?? new Dictionary<string, IExpression>();
			MethodName = name;
			Args.Add("self", Value);
			Type = type;
		}

		public override ILsnValue Eval(IInterpreter i)
		{
			var args = Args.Select(p => new KeyValuePair<string, ILsnValue>(p.Key, p.Value.Eval(i))).ToDictionary();
			var self = Value.Eval(i);
			args.Add("self", self);
			var fn = M ?? self.Type.Methods[MethodName];
			if (!fn.HandlesScope) i.EnterFunctionScope(fn.Environment);
			var val = fn.Eval(args, i);
			if (!fn.HandlesScope) i.ExitFunctionScope();
			return val;
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

	}
}
