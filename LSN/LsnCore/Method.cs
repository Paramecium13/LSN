using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using System.Linq;
using LsnCore.Types;

namespace LsnCore
{
	[Serializable]
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
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <param name="expression"> The object the method is called on</param>
		/// <param name="included"></param>
		/// <returns></returns>
		public MethodCall CreateMethodCall(IList<Tuple<string, IExpression>> args, IExpression expression, bool included = false)
		{
			var dict = args.ToDictionary(t => t.Item1, t => t.Item2);
			var argsArray = new IExpression[Parameters.Count];
			argsArray[0] = expression;
			foreach (var param in Parameters)
			{
				if (param.Name != "self")
					argsArray[param.Index] = dict.ContainsKey(param.Name) ? dict[param.Name] : param.DefaultValue;
			}
			return included ? new MethodCall(this, argsArray) : new MethodCall(Name, ReturnType, argsArray);
		}

	}
}
