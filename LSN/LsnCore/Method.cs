using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
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

		protected Method(LsnType type, LsnType returnType, string name, IList<Parameter> parameters)
			:base(new FunctionSignature(parameters, name, returnType?.Id))
		{
			TypeId = type.Id;
		}

		protected Method(LsnType type, TypeId returnType, string name, IList<Parameter> parameters)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			TypeId = type.Id;
		}

		protected Method(TypeId type, TypeId returnType, string name, IList<Parameter> parameters)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			TypeId = type;
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="args"></param>
		/// <param name="expression"> The object the method is called on</param>
		/// <returns></returns>
		public virtual IExpression CreateMethodCall(IList<Tuple<string, IExpression>> args, IExpression expression)
		{
			// This requires that the call use the named parameters. Do something like if (args.Any(a=>a.Item1=="")){...}
			// to check if it uses parameters by position, rather than name...
			var argsArray = new IExpression[Parameters.Count];
			argsArray[0] = expression;
			for(int i = 1; i < Parameters.Count; i++)
			{
				var a = args.FirstOrDefault(arg => arg.Item1 == Parameters[i].Name);
				if (a != null)
					argsArray[i] = a.Item2;
				else if (i < args.Count)
					argsArray[i] = args[i].Item2;
				else if (!Parameters[i].DefaultValue.IsNull)
					argsArray[i] = Parameters[i].DefaultValue;
				else throw new ArgumentException(nameof(args));
			}
			return new MethodCall(this, argsArray);
		}

	}
}
