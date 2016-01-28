using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	public class BoundedFunction : Function
	{
		private Func<Dictionary<string, ILSN_Value>, ILSN_Value> Bound;

		public override bool HandlesScope { get { return false; } }

		public BoundedFunction(Func<Dictionary<string, ILSN_Value>, ILSN_Value> b, List<Parameter> paramaters, LSN_Type returnType)
		{
			Bound = b;
			Parameters = paramaters;
			ReturnType = returnType;
		}

		public override ILSN_Value Eval(Dictionary<string, ILSN_Value> args, IInterpreter i)
			=> Bound(args);
		

	}
}
