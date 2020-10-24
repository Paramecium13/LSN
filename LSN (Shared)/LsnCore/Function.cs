using LsnCore.Serialization;
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
		/// <summary>
		/// The function's signature
		/// </summary>
		public readonly FunctionSignature Signature;

		/// <summary>
		/// Gets the function's name.
		/// </summary>
		public string Name => Signature.Name;

		/// <summary>
		/// Gets the function's return type, or null if it doesn't return anything (aka returns '()' or <see langword="void"/>).
		/// </summary>
		public TypeId ReturnType => Signature.ReturnType;

		/// <summary>
		/// Gets the function's parameters.
		/// </summary>
		public IReadOnlyList<Parameter> Parameters => Signature.Parameters;

		/// <summary>
		/// Gets or sets the size of the function stack for this function.
		/// </summary>
		public int StackSize { get; set; } = -1; // Should only be set in LSNr.
												 // If the value is -1, this indicates that the stack size was never set.

		/// <summary>
		/// Initializes a new instance of the <see cref="Function"/> class.
		/// </summary>
		/// <param name="signature">The signature.</param>
		protected Function(FunctionSignature signature)
		{
			Signature = signature;
		}

		/// <summary>
		/// The resource file path
		/// </summary>
		protected string _ResourceFilePath;
		public string ResourceFilePath { get => _ResourceFilePath;
			protected set => _ResourceFilePath = value;
		}
		
#if CORE
		public abstract LsnValue Eval(LsnValue[] args, IInterpreter i);
#endif
	}

	/// <summary>
	/// A parameter for a function or method.
	/// </summary>
	public class Parameter : IEquatable<Parameter>
	{
		/// <summary>
		/// The name of the parameter.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The type of the parameter.
		/// </summary>
		public readonly TypeId Type;

		/// <summary>
		/// The parameter's default value. If it doesn't have a default value, <see cref="LsnValue.Nil"/>.
		/// </summary>
		public readonly LsnValue DefaultValue;

		/// <summary>
		/// The parameter's index.
		/// </summary>
		public readonly ushort Index;

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameter"/> class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="type">The type of the parameter.</param>
		/// <param name="val">The default value of the parameter, or <see cref="LsnValue.Nil"/>.</param>
		/// <param name="i">The index of the parameter.</param>
		public Parameter(string name, TypeId type, LsnValue val, ushort i)
		{
			Name = name;
			Type = type;
			DefaultValue = val;
			Index = i;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameter"/> class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="type">The type of the parameter.</param>
		/// <param name="val">The default value of the parameter, or <see cref="LsnValue.Nil"/>.</param>
		/// <param name="i">The index of the parameter.</param>
		public Parameter(string name, LsnType type, LsnValue val, ushort i)
		{
			Name = name;
			Type = type.Id;
			DefaultValue = val;
			Index = i;
		}

		/// <inheritdoc/>
		public bool Equals(Parameter other)
			=> Name == other.Name && Type == other.Type && DefaultValue.Equals(other.DefaultValue) && Index == other.Index;

		/// <summary>
		/// Serializes the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="resourceSerializer">The resource serializer.</param>
		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(Name);
			resourceSerializer.WriteTypeId(Type, writer);
			writer.Write(!DefaultValue.IsNull);
		}

		/// <summary>
		/// Reads a parameter.
		/// </summary>
		/// <param name="index">The <see cref="Index"/> of the parameter. This is determined by its order.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="typeContainer">The type container.</param>
		/// <returns></returns>
		public static Parameter Read(ushort index, BinaryDataReader reader, ITypeIdContainer typeContainer)
		{
			var name = reader.ReadString();
			var typeId = typeContainer.GetTypeId(reader.ReadUInt16());
			var hasDefault = reader.ReadBoolean();
			/*if (hasDefault)
				throw new ApplicationException();*/
			return new Parameter(name, typeId, LsnValue.Nil, index);
		}

		/// <summary>
		/// Reads a parameter.
		/// </summary>
		/// <param name="index">The <see cref="Index"/> of the parameter. This is determined by its order.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="resourceDeserializer">The resource deserializer.</param>
		/// <returns></returns>
		public static Parameter Read(ushort index, BinaryDataReader reader, ResourceDeserializer resourceDeserializer)
		{
			var name = reader.ReadString();
			var typeId = resourceDeserializer.LoadTypeId(reader);
			var hasDefault = reader.ReadBoolean();
			/*if (hasDefault)
				throw new ApplicationException();*/
			return new Parameter(name, typeId, LsnValue.Nil, index);
		}

	}
}
