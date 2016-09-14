using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	/// <summary>
	/// A function.
	/// </summary>
	[Serializable]
	public abstract class Function
	{
		public string Name { get; protected set; }
		public LsnType ReturnType { get; protected set; }
		private readonly List<Parameter> _Parameters;
		public IReadOnlyList<Parameter> Parameters => _Parameters;
		public int StackSize { get; set; }


		public Function(List<Parameter> parameters)
		{
			_Parameters = parameters;
		}

		public abstract bool HandlesScope { get; }

		protected LsnEnvironment _Environment;
        public LsnEnvironment Environment { get { return _Environment; } protected set { _Environment = value; } }

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


		public virtual FunctionCall CreateCall(IList<Tuple<string,IExpression>> args, bool included = false)
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

		public abstract ILsnValue Eval(Dictionary<string, ILsnValue> args, IInterpreter i);

	}

	/// <summary>
	/// A parameter for a function or method.
	/// </summary>
	[Serializable]
	public class Parameter
	{
		public string Name;
		public LsnType Type;
		public ILsnValue DefaultValue;
		public ushort Index;

		public Parameter(string name, LsnType type, ILsnValue val, ushort i)
		{
			Name = name;
			Type = type;
			DefaultValue = val;
			Index = i;
		}

	}
}
