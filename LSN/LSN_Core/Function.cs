using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	/// <summary>
	/// A function.
	/// </summary>
	public abstract class Function
	{
		public string Name;
		public LSN_Type ReturnType;
		public List<Parameter> parameters;

		/// <summary>
		/// A parameter.
		/// </summary>
		public class Parameter
		{
			public string Name;
			public LSN_Type Type;
			public ILSN_Value DefaultValue;
		}

		//Todo: create a function call expression.
		public virtual Expression CreateCall(Dictionary<string,IExpression> args, bool throwOnInvalidName = false)
		{
			// Check for invalid names.
			if(throwOnInvalidName)
			{
				foreach(var name in args.Keys)
				{
					if (!parameters.Any(p => p.Name == name))
						throw new ApplicationException($"No argument named {name} was found for {this.Name}.");
				}
			}
			// Check type.
			Dictionary<string, IExpression> fullArgs = new Dictionary<string, IExpression>();
			foreach(var param in parameters)
			{
				var name = param.Name;
				if(args.ContainsKey(name))
				{
					if (!param.Type.Subsumes(args[name].Type))
						throw new ApplicationException(
						$"Expected {param.Type.Name} or a valid subtype for parameter {name} recieved {args[name].Type.Name}.");
					fullArgs.Add(name, args[name]);
					continue;
				}// implicit else
				if(param.DefaultValue == null) // This argument does not have a default value.
					throw new ApplicationException($"The parameter {param.Name} of {this.Name} must be provided a value.");
				fullArgs.Add(name, param.DefaultValue);
			}
			return  new FunctionCall(this, fullArgs);
		}

		public abstract ILSN_Value Eval(Dictionary<string, IExpression> args, IInterpreter i);

	}
}
