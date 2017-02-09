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

		public BoundedFunction(Func<LsnValue[], LsnValue> b, List<Parameter> parameters, LsnType returnType, string name)
			:base(parameters)
		{
			Bound = b;
			ReturnType = returnType?.Id;
			Name = name;
		}

		public BoundedFunction(Func<LsnValue[], LsnValue> b, List<Parameter> parameters, TypeId returnType, string name)
			: base(parameters)
		{
			Bound = b;
			ReturnType = returnType;
			Name = name;
		}

		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
			=> Bound(args);
		

	}
}
