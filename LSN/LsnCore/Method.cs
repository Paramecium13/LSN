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
		public readonly LsnType Type; // ToDo: Replace with TypeId.


		public Method(LsnType type, LsnType returnType, List<Parameter> paramaters)
			:base(paramaters)
		{
			Type = type;
			ReturnType = returnType?.Id;
		}


		public Method(LsnType type, TypeId returnType, List<Parameter> paramaters)
			: base(paramaters)
		{
			Type = type;
			ReturnType = returnType;
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
