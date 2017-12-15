using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;
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
	public abstract class Function
	{
		public readonly FunctionSignature Signature;

		public string Name => Signature.Name;

		public TypeId ReturnType => Signature.ReturnType;

		public IReadOnlyList<Parameter> Parameters => Signature.Parameters;

		public int StackSize { get; set; } = -1; // Should only be set in LSNr.
											// If the value is -1, this indicates that the stack size was never set.

		protected Function(FunctionSignature signature)
		{
			Signature = signature;
		}

		/// <summary>
		/// Does this function handle the scope and stack by itself (i.e. without any calls to the interpreter)?
		/// Typically true for bound functions and methods.
		/// </summary>
		public abstract bool HandlesScope { get; }

		protected string _ResourceFilePath; //TODO: Give this a value!!!
		public string ResourceFilePath { get { return _ResourceFilePath; } protected set { _ResourceFilePath = value; } }

		public virtual FunctionCall CreateCall(IList<Tuple<string,IExpression>> args, bool included = false)
		{
			var argsArray = Signature.CreateArgsArray(args);

			return new FunctionCall(this, argsArray);
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

		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write(Name);
			writer.Write(Type.Name);
			writer.Write(!DefaultValue.IsNull);
		}


		public static Parameter Read(ushort index, BinaryDataReader reader, ITypeIdContainer typeContainer)
		{
			var name = reader.ReadString();
			var typeName = reader.ReadString();
			var hasDefault = reader.ReadBoolean();
			/*if (hasDefault)
				throw new ApplicationException();*/
			return new Parameter(name, typeContainer.GetTypeId(typeName), LsnValue.Nil, index);
		}

	}
}
