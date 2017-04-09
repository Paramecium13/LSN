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
		public readonly FunctionSignature Signature;


		public string Name => Signature.Name;


		public TypeId ReturnType => Signature.ReturnType;


		public IReadOnlyList<Parameter> Parameters => Signature.Parameters;


		public int StackSize { get; set; } // Should only be set in LSNr.


		protected Function(FunctionSignature signature)
		{
			Signature = signature;
		}

		/// <summary>
		/// Does this function handle the scope and stack by itself (i.e. without any calls to the interpreter)?
		/// Typically true for bound functions and methods.
		/// </summary>
		public abstract bool HandlesScope { get; }


		protected LsnEnvironment _Environment; //TODO: Give this a value!!!
		public LsnEnvironment Environment { get { return _Environment; } protected set { _Environment = value; } }


		public virtual FunctionCall CreateCall(IList<Tuple<string,IExpression>> args, bool included = false)
		{
			var argsArray = Signature.CreateArgsArray(args);

			return new FunctionCall(this, argsArray, included);
		}

		public abstract LsnValue Eval(LsnValue[] args, IInterpreter i);

	}

	/// <summary>
	/// A parameter for a function or method.
	/// </summary>
	[Serializable]
	public class Parameter : IEquatable<Parameter>
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

		public bool Equals(Parameter other)
			=> Name == other.Name && Type == other.Type && DefaultValue.Equals(other.DefaultValue) && Index == other.Index;
	}
}
