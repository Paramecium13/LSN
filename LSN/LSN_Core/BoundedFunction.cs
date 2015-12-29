using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	public class BoundedFunction : Function
	{
		private Func<Dictionary<string, ILSN_Value>, ILSN_Value> Bound;


		public BoundedFunction(Func<Dictionary<string, ILSN_Value>, ILSN_Value> b)
		{
			Bound = b;
		}

		public override ILSN_Value Eval(Dictionary<string, IExpression> args, IInterpreter i)
		{
			var argVals = new Dictionary<string, ILSN_Value>();
			foreach (var pair in args) argVals.Add(pair.Key, pair.Value.Eval(i));
			return Bound(argVals);
        }

	}
}
