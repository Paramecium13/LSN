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
	[Serializable]
	public abstract class Function
	{
		public string Name { get; protected set; }
		public LSN_Type ReturnType { get; protected set; }
		public List<Parameter> Parameters;
		

		public abstract bool HandlesScope { get; }

		protected LSN_Environment _Environment;
        public LSN_Environment Environment { get { return _Environment; } protected set { _Environment = value; } }

		//Todo: create a function call expression.
		public virtual IExpression CreateCall(Dictionary<string,IExpression> args, bool throwOnInvalidName = false, bool included = false)
		{
			// Check for invalid names.
			if(throwOnInvalidName)
			{
				foreach(var name in args.Keys)
				{
					if (!Parameters.Any(p => p.Name == name))
						throw new ApplicationException($"No argument named {name} was found for {this.Name}.");
				}
			}
			// Check type.
			Dictionary<string, IExpression> fullArgs = new Dictionary<string, IExpression>();
			foreach(var param in Parameters)
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
			return new FunctionCall(this, fullArgs, included);
		}


		public virtual FunctionCall CreateCall(List<Tuple<string,IExpression>> args, bool included = false)
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
				foreach(var param in Parameters)
				{
					if (dict.ContainsKey(param.Name)) continue;
					if (param.DefaultValue == null) // This argument does not have a default value.
						throw new ApplicationException($"The parameter {param.Name} of {this.Name} must be provided a value.");
					dict.Add(param.Name, param.DefaultValue);
				}
			}
			return new FunctionCall(this,dict, included);
		}

		public abstract ILSN_Value Eval(Dictionary<string, ILSN_Value> args, IInterpreter i);

	}

	/// <summary>
	/// A parameter for a function or method.
	/// </summary>
	public class Parameter
	{
		public string Name;
		public LSN_Type Type;
		public ILSN_Value DefaultValue;
		public ushort Index;

		public Parameter(string name, LSN_Type type, ILSN_Value val, ushort i)
		{
			Name = name;
			Type = type;
			DefaultValue = val;
			Index = i;
		}

	}
}
