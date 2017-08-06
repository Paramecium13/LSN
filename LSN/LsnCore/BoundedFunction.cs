using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LsnCore
{
	[Serializable]
	public class BoundedFunction : Function
	{
		private Func<LsnValue[], LsnValue> Bound;

		public override bool HandlesScope { get { return false; } }

		public BoundedFunction(Func<LsnValue[], LsnValue> bound, List<Parameter> parameters, LsnType returnType, string name)
			:base(new FunctionSignature(parameters,name,returnType?.Id))
		{
			Bound = bound;
		}

		public BoundedFunction(Func<LsnValue[], LsnValue> bound, List<Parameter> parameters, TypeId returnType, string name)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			Bound = bound;
		}

		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
			=> Bound(args);
		

	}
}
