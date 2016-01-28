﻿using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	public class BoundedMethod : Method
	{
		public override bool HandlesScope { get { return false; } }

		private Func<Dictionary<string, ILSN_Value>, ILSN_Value> Bound;

		public override ILSN_Value Eval(Dictionary<string, ILSN_Value> args, IInterpreter i)
			=> Bound(args);
		
	}
}
