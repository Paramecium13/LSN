using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;

namespace LsnCore
{
	[Serializable]
	public class BoundedFunction : Function
	{
		private Func<Dictionary<string, ILsnValue>, ILsnValue> Bound;

		public override bool HandlesScope { get { return false; } }

		public BoundedFunction(Func<Dictionary<string, ILsnValue>, ILsnValue> b, List<Parameter> parameters, LsnType returnType)
			:base(parameters)
		{
			Bound = b;
			ReturnType = returnType;
		}

		public override ILsnValue Eval(Dictionary<string, ILsnValue> args, IInterpreter i)
			=> Bound(args);
		

	}
}
