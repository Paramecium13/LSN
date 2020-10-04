using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LsnCore
{
	public class BoundedFunction : Function
	{
		private Func<LsnValue[], LsnValue> Bound;

		public BoundedFunction(Func<LsnValue[], LsnValue> bound, IReadOnlyList<Parameter> parameters, LsnType returnType, string name)
			:base(new FunctionSignature(parameters,name,returnType?.Id))
		{
#if CORE
			if (bound == null)
				throw new ArgumentNullException(nameof(bound));
#endif
			Bound = bound;
		}

#if CORE
		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
			=> Bound(args);
#endif
	}

#if CORE
	public class BoundedFunctionWithInterpreter : Function
	{
		private Func<IInterpreter, LsnValue[], LsnValue> Bound;

		public BoundedFunctionWithInterpreter(Func<IInterpreter, LsnValue[], LsnValue> bound, List<Parameter> parameters, TypeId returnType, string name)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			Bound = bound ?? throw new ArgumentNullException(nameof(bound));
		}

		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
			=> Bound(i,args);
	}
#else
	public class BoundedFunctionWithInterpreter : Function
	{
		private Func<object, LsnValue[], LsnValue> Bound;

		public BoundedFunctionWithInterpreter(Func<object, LsnValue[], LsnValue> bound, List<Parameter> parameters, TypeId returnType, string name)
			: base(new FunctionSignature(parameters, name, returnType))
		{
#if CORE
			if (bound == null)
				throw new ArgumentNullException(nameof(bound));
#endif
			Bound = bound;
		}
	}
#endif
}
