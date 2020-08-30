using System;
using System.Collections.Generic;
using System.Text;
#if LSNR
using LsnCore.Expressions;
#endif
using System.Linq;
using LsnCore.Types;

namespace LsnCore
{
	public abstract class Method : Function
	{
		/// <summary>
		/// The type this method is a member of.
		/// </summary>
		public readonly TypeId TypeId;

		protected Method(LsnType type, LsnType returnType, string name, IReadOnlyList<Parameter> parameters)
			: base(new FunctionSignature(parameters, name, returnType?.Id))
		{
			TypeId = type.Id;
		}

		protected Method(LsnType type, TypeId returnType, string name, IReadOnlyList<Parameter> parameters)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			TypeId = type.Id;
		}

		protected Method(TypeId type, TypeId returnType, string name, IReadOnlyList<Parameter> parameters)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			TypeId = type;
		}
		#if LSNR
		public virtual IExpression CreateMethodCall(IExpression[] args)
			=> new MethodCall(this, args);
		#endif
	}
}
