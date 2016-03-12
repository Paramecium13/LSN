using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	[Serializable]
	public class BoundedMethod : Method
	{
		public override bool HandlesScope => false;

		private readonly Func<Dictionary<string, ILSN_Value>, ILSN_Value> Bound;

		/// <summary>
		/// Create a new BoundedMethod.
		/// </summary>
		/// <param name="type">The LSN_Type this is a method of.</param>
		/// <param name="returnType">The LSN_Type of the returned value (null if void).</param>
		/// <param name="bound">The .NET function.</param>
		public BoundedMethod(LSN_Type type, LSN_Type returnType, Func<Dictionary<string, ILSN_Value>, ILSN_Value> bound, List<Parameter> paramaters = null)
			:base(type,returnType, paramaters ?? new List<Parameter>() { new Parameter("self",type,null,0)})
		{
			Bound = bound;
		}

		public override ILSN_Value Eval(Dictionary<string, ILSN_Value> args, IInterpreter i)
			=> Bound(args);
		
	}
}
