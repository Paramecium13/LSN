using LsnCore.Expressions;
using LsnCore.Types;
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
		public TypeId ReturnType { get; protected set; }
		private readonly List<Parameter> _Parameters;
		public IReadOnlyList<Parameter> Parameters => _Parameters;
		public int StackSize { get; set; }


		public Function(IList<Parameter> parameters)
		{
			_Parameters = parameters.ToList();
		}

		/// <summary>
		/// Does this function handle the scope and stack by itself (i.e. without any calls to the interpreter)?
		/// Typically true for bound functions and methods.
		/// </summary>
		public abstract bool HandlesScope { get; }

		protected LsnEnvironment _Environment;
        public LsnEnvironment Environment { get { return _Environment; } protected set { _Environment = value; } }


		public virtual FunctionCall CreateCall(IList<Tuple<string,IExpression>> args, bool included = false)
		{
			var dict = args.ToDictionary(t => t.Item1, t => t.Item2);
			var argsArray = new IExpression[Parameters.Count];

			foreach (var param in Parameters)
				argsArray[param.Index] = dict.ContainsKey(param.Name) ? dict[param.Name] : param.DefaultValue;

			return new FunctionCall(this, argsArray, included);
		}

		public abstract LsnValue Eval(LsnValue[] args, IInterpreter i);

	}

	/// <summary>
	/// A parameter for a function or method.
	/// </summary>
	[Serializable]
	public class Parameter
	{
		public readonly string Name;
		public readonly TypeId Type;
		public readonly LsnValue DefaultValue;
		public readonly ushort Index;

		public Parameter(string name, TypeId type, LsnValue val, ushort i)
		{
			Name = name;
			Type = type;
			DefaultValue = val;
			Index = i;
		}

		public Parameter(string name, LsnType type, LsnValue val, ushort i)
		{
			Name = name;
			Type = type.Id;
			DefaultValue = val;
			Index = i;
		}

	}
}
