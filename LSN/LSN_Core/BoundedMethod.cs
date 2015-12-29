using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	public class BoundedMethod : Method
	{
		public override ILSN_Value Eval(Dictionary<string, IExpression> args, IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
}
