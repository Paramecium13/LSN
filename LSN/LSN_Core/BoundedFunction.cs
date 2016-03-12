using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	[Serializable]
	public class BoundedFunction : Function
	{
		private Func<Dictionary<string, ILSN_Value>, ILSN_Value> Bound;

		public override bool HandlesScope { get { return false; } }

		public BoundedFunction(Func<Dictionary<string, ILSN_Value>, ILSN_Value> b, List<Parameter> parameters, LSN_Type returnType)
			:base(parameters)
		{
			Bound = b;
			ReturnType = returnType;
		}

		public override ILSN_Value Eval(Dictionary<string, ILSN_Value> args, IInterpreter i)
			=> Bound(args);
		

	}
}
