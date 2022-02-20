using System;
using System.Collections.Generic;

namespace LsnCore
{
	/// <summary>
	/// A <see cref="Method"/> that is implemented via .Net code.
	/// </summary>
	public class BoundedMethod : Method
	{
		/// <summary>
		/// The bound function that implements the method.
		/// </summary>
		private readonly Func<LsnValue[], LsnValue> Bound;

		/// <summary>
		/// Create a new BoundedMethod.
		/// </summary>
		/// <param name="type">The LSN_Type this is a method of.</param>
		/// <param name="returnType">The LSN_Type of the returned value (null if void).</param>
		/// <param name="bound">The .NET function.</param>
		/// <param name="name">The name of the method</param>
		/// <param name="paramaters">The method's parameters</param>
		public BoundedMethod(LsnType type, LsnType returnType, Func<LsnValue[], LsnValue> bound, string name, List<Parameter> paramaters = null)
			:base(type,returnType, name, paramaters ?? new List<Parameter> { new Parameter("self",type.Id, LsnValue.Nil, 0)})
		{
			Bound = bound;
		}
#if CORE
		//public override LsnValue Eval(LsnValue[] args, IInterpreter i) => Bound(args);

		/// <summary>
		/// Evaluate the method.
		/// </summary>
		/// <param name="args">The method's arguments.</param>
		/// <returns></returns>
		public LsnValue Eval(LsnValue[] args) => Bound(args);

		public LsnValue Eval(Stack<LsnValue> evalStack) => throw new NotImplementedException();
#endif
	}
}
