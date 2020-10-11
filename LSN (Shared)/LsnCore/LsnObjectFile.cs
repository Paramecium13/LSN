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
	/// Includes the name of the containing file.
	/// </summary>
	public readonly struct FullProcedureDefinition
	{
		public readonly ProcedureDefinition Definition;
		public readonly string FilePath;
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
	public readonly struct SignatureStub
	{
		/// <summary>
		/// The name of the procedure.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The number of parameters the procedure takes, including 'self'.
		/// </summary>
		public readonly int NumberOfParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="SignatureStub"/> struct.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="numberOfParameters">The number of parameters.</param>
		public SignatureStub(string name, int numberOfParameters)
		{
			Name = name;
			NumberOfParameters = numberOfParameters;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SignatureStub"/> struct.
		/// </summary>
		/// <param name="signature">The function signature to base this stub on.</param>
		public SignatureStub(FunctionSignature signature) : this(signature.Name, signature.Parameters.Count)
		{}
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
		/// The signature stubs of procedures called by code in this file.
		/// </summary>
		private readonly SignatureStub[] SignatureStubs;

		private readonly IReadOnlyDictionary<string, int> ProcedureIndexLookup;

		private readonly ProcedureInfo[] ContainedProcedures;

		private readonly LsnObjectFile[] ReferencedFiles;

		/// <summary>
		/// Gets the name of this file.
		/// </summary>
		public string FileName { get; }

		internal Instruction[] Code { get; }
		
		internal LsnValue GetDouble(ushort index) => new LsnValue(ConstTable.GetDouble(index));

		/// <summary>
		/// Gets a string constant.
		/// </summary>
		/// <param name="index">The index.</param>
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
