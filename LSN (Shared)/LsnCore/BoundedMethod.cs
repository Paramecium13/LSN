﻿using System;
using System.Collections.Generic;

namespace LsnCore
{
	public class BoundedMethod : Method
	{
		private readonly Func<LsnValue[], LsnValue> Bound;

		/// <summary>
		/// Create a new BoundedMethod.
		/// </summary>
		/// <param name="type">The LSN_Type this is a method of.</param>
		/// <param name="returnType">The LSN_Type of the returned value (null if void).</param>
		/// <param name="bound">The .NET function.</param>
		/// <param name="name"></param>
		/// <param name="paramaters"></param>
		public BoundedMethod(LsnType type, LsnType returnType, Func<LsnValue[], LsnValue> bound, string name, List<Parameter> paramaters = null)
			:base(type,returnType, name, paramaters ?? new List<Parameter> { new Parameter("self",type.Id, LsnValue.Nil, 0)})
		{
			Bound = bound;
		}
#if CORE
		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
			=> Bound(args);
#endif
	}
}
