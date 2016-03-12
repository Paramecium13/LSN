using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;
using System.Linq;

namespace LSN_Core
{
	[Serializable]
	public abstract class Method : Function
	{
		/// <summary>
		/// The type this method is a member of.
		/// </summary>
		public readonly LSN_Type Type;

		public Method(LSN_Type type,LSN_Type returnType, List<Parameter> paramaters)
			:base(paramaters)
		{
			Type = type;
			ReturnType = returnType;
		}
		

		public MethodCall CreateMethodCall(List<Tuple<string, IExpression>> args, IExpression expression, bool included = false)
		{
			var dict = new Dictionary<string, IExpression>();
			string name;
			for (int i = 0; i < args.Count; i++)
			{
				IExpression expr = args[i].Item2;
                if (args[i].Item1 != null && args[i].Item1 != "")
				{
					name = args[i].Item1;
					if (!Parameters.Any(p => p.Name == name))
						throw new ApplicationException($"Cannot find a parameter named {name}.");//return null;// Log an error or something.
					var param = Parameters.Where(p => p.Name == name).First();
					if (!param.Type.Subsumes(args[i].Item2.Type))
						throw new ApplicationException(
						$"Expected {param.Type.Name} or a valid subtype for parameter {name} recieved {expr.Type.Name}.");
					dict.Add(name, args[i].Item2);
				}
				else
				{
					var param = Parameters.Where(p => p.Index == i).FirstOrDefault() ?? Parameters[i];
					if (!param.Type.Subsumes(args[i].Item2.Type))
					throw new ApplicationException(
						$"Expected {param.Type.Name} or a valid subtype for parameter {args[i].Item1} recieved {expr.Type.Name}.");
					dict.Add(param.Name, args[i].Item2);
				}
			}
			if (dict.Count < Parameters.Count)
			{
				foreach (var param in Parameters)
				{
					if (dict.ContainsKey(param.Name)) continue;
					if (param.DefaultValue == null) // This argument does not have a default value.
						throw new ApplicationException($"The parameter {param.Name} of {this.Name} must be provided a value.");
					dict.Add(param.Name, param.DefaultValue);
				}
			}
			return included ? new MethodCall(this, expression, dict) : new MethodCall(Name, expression, dict);
		}

	}
}
