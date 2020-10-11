using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	public readonly struct ProcedureDefinition
	{
		public readonly string Name;

		/// <summary>
		/// The offset of the code for this procedure
		/// </summary>
		public readonly int CodeOffset;

		/// <summary>
		/// The (function) stack size of this procedure.
		/// </summary>
		public readonly ushort StackSize;

		/// <summary>
		/// The parameters
		/// </summary>
		public readonly Parameter[] Parameters;
	}

	/// <summary>
	/// Information about a procedure implemented in LSN. Used by the virtual machine to call the procedure.
	/// </summary>
	public readonly struct ProcedureInfo
	{
		/// <summary>
		/// The offset of the code for this procedure
		/// </summary>
		public readonly int CodeOffset;

		/// <summary>
		/// The (function) stack size of this procedure.
		/// </summary>
		public readonly ushort StackSize;

		/// <summary>
		/// The file that contains this procedure.
		/// </summary>
		public readonly LsnObjectFile File;

		/// <summary>
		/// The number of parameters this procedure has.
		/// </summary>
		public readonly ushort NumberOfParameters;

		public ProcedureInfo(int offset, ushort stackSize, ushort numberOfParameters, LsnObjectFile file)
		{
			CodeOffset = offset; StackSize = stackSize;
			NumberOfParameters = numberOfParameters; File = file;
		}
	}

	/// <summary>
	/// Contains the information that the Virtual Machine needs to call a host interface method.
	/// </summary>
	/// <remarks>
	/// If/when the VM transitions to using Handles for methods, types, & stuff, this will contain
	/// the method's handle in place of its name.
	/// </remarks>
	public readonly struct SignatureStub : IEquatable<SignatureStub>
	{
		/// <summary>
		/// The name of the host interface type this method was defined in.
		/// </summary>
		public readonly string HostInterfaceName;

		/// <summary>
		/// The identifier of the method. Without any overloading (and thus without name mangling), this
		/// is just the name of the method.
		/// </summary>
		public readonly string Identifier;

		/// <summary>
		/// The number of parameters the method takes, including 'self'.
		/// </summary>
		public readonly int NumberOfParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="SignatureStub"/> struct.
		/// </summary>
		/// <param name="identifier">The identifier of the method.</param>
		/// <param name="numberOfParameters">The number of parameters the method takes, including 'self'.</param>
		/// <param name="hostInterfaceName"> The name of the host interface this method belongs to. </param>
		public SignatureStub(string identifier, int numberOfParameters, string hostInterfaceName)
		{
			Identifier = identifier;
			NumberOfParameters = numberOfParameters;
			HostInterfaceName = hostInterfaceName;
		}

		/// <summary>Initializes a new instance of the <see cref="SignatureStub" /> struct.</summary>
		/// <param name="signature">The function signature to base this stub on.</param>
		/// <param name="hostInterfaceName"> The name of the host interface this method belongs to. </param>
		public SignatureStub(FunctionSignature signature, string hostInterfaceName) : this(signature.Name, signature.Parameters.Count, hostInterfaceName)
		{}

		/// <inheritdoc />
		/// <remarks>
		/// Does not take into account <see cref="NumberOfParameters"/>.
		/// A host interface cannot have two different methods with the same identifier.
		/// </remarks>
		public bool Equals(SignatureStub other)
		{
			return HostInterfaceName == other.HostInterfaceName && Identifier == other.Identifier;
		}

		/// <inheritdoc />
		/// <remarks>
		/// Does not take into account <see cref="NumberOfParameters"/>.
		/// A host interface cannot have two different methods with the same identifier.
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is SignatureStub other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (HostInterfaceName.GetHashCode() * 397) ^ Identifier.GetHashCode();
			}
		}
	}

	public enum ProcedureClassification
	{
		Function,
		Method,
		Constructor,
		EventHandler,
	}

	/// <summary>
	/// 
	/// </summary>
	public class LsnObjectFile
	{
		/// <summary>
		/// The constant table
		/// </summary>
		private readonly ConstTableStruct ConstTable;

		/// <summary>
		/// The referenced types
		/// </summary>
		private readonly TypeId[] ReferencedTypes;

		/// <summary>
		/// The defined types
		/// </summary>
		private readonly TypeId[] DefinedTypes;

		/// <summary>
		/// A lookup of types defined in this file by their names.
		/// </summary>
		private readonly IReadOnlyDictionary<string, TypeId> DefinedTypesLookup;

		/// <summary>
		/// The signature stubs of host interface methods called by code in this file.
		/// </summary>
		private readonly SignatureStub[] SignatureStubs;

		/// <summary>
		/// A lookup of procedure indexes in <see cref="ContainedProcedures"/> by procedure name.
		/// </summary>
		private readonly IReadOnlyDictionary<string, int> ProcedureIndexLookup;

		/// <summary>
		/// The procedures contained in this file.
		/// </summary>
		private readonly ProcedureInfo[] ContainedProcedures;

		/// <summary>
		/// The files referenced by this file.
		/// </summary>
		private readonly LsnObjectFile[] ReferencedFiles;

		/// <summary>
		/// Gets the path to this file, relative to the LSN-Environment-Root, excluding any file extension.
		/// </summary>
		public string FilePath { get; }

		internal Instruction[] Code { get; }

		/// <summary>
		/// Gets a double constant.
		/// </summary>
		/// <param name="index">The index in the double constant table.</param>
		/// <returns></returns>
		internal LsnValue GetDouble(ushort index) => new LsnValue(ConstTable.GetDouble(index));

		/// <summary>
		/// Gets a string constant.
		/// </summary>
		/// <param name="index">The index in the string constant table.</param>
		/// <returns></returns>
		internal LsnValue GetString(ushort index) => new LsnValue(ConstTable.GetString(index));

		/// <summary>
		/// A constant table
		/// </summary>
		private readonly struct ConstTableStruct
		{
			private readonly double[] ConstDoubles;
			private readonly StringValue[] ConstStrings;

			internal ConstTableStruct(double[] doubles, string[] strings)
			{
				ConstDoubles = doubles;
				ConstStrings = new StringValue[strings.Length];
				for (int i = 0; i < strings.Length; i++)
				{
					ConstStrings[i] = new StringValue(strings[i]);
				}
			}

			internal double GetDouble(ushort index) => ConstDoubles[index];
			internal StringValue GetString(ushort index) => ConstStrings[index];
		}


		/// <summary>
		/// Gets a string that is the name of something
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal string GetIdentifierString(ushort index) => throw new NotImplementedException();

		internal SignatureStub GetSignatureStub(ushort index) => SignatureStubs[index];

		/// <summary>
		/// Gets the id of a type used by code in this file. A negative value of <paramref name="index"/> indicates it is a locally defined type.
		/// </summary>
		internal TypeId GetUsedTypeId(short index) => index > 0 ? ReferencedTypes[index]: DefinedTypes[-index];

		/// <summary>
		/// Gets a type used by code in this file. A negative value of <paramref name="index"/> indicates it is a locally defined type.
		/// </summary>
		internal LsnType GetUsedType(short index) => GetUsedTypeId(index).Type;
		
		/// <summary>
		/// Gets a type contained in this file.
		/// </summary>
		internal TypeId GetContainedType(string name) => DefinedTypesLookup[name];

		/// <summary>
		/// Get an LSN procedure contained in this file.
		/// </summary>
		/// <param name="index"></param>
		internal ProcedureInfo GetProcedure(ushort index) => ContainedProcedures[index];

		internal ProcedureInfo GetProcedure(string name) => ContainedProcedures[ProcedureIndexLookup[name]];

		internal LsnObjectFile GetFile(ushort index) => ReferencedFiles[index];
	}

}
