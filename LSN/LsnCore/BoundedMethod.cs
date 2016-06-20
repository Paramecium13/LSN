using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;

namespace LsnCore
{
	[Serializable]
	public class BoundedMethod : Method
	{
		public override bool HandlesScope => false;

		private readonly Func<Dictionary<string, ILsnValue>, ILsnValue> Bound;

		/// <summary>
		/// Create a new BoundedMethod.
		/// </summary>
		/// <param name="type">The LSN_Type this is a method of.</param>
		/// <param name="returnType">The LSN_Type of the returned value (null if void).</param>
		/// <param name="bound">The .NET function.</param>
		public BoundedMethod(LsnType type, LsnType returnType, Func<Dictionary<string, ILsnValue>, ILsnValue> bound, List<Parameter> paramaters = null)
			:base(type,returnType, paramaters ?? new List<Parameter>() { new Parameter("self",type,null,0)})
		{
			Bound = bound;
		}

		public override ILsnValue Eval(Dictionary<string, ILsnValue> args, IInterpreter i)
			=> Bound(args);
		
	}
}
