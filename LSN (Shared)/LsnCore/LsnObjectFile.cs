﻿using LsnCore.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Linq;

namespace LsnCore.Interpretation
{
	/*
	 * File Structure Proposal
	 *	Header:
	 *		Info about this file...
	 *	Data Section (May be shared by multiple files to save space...?):
	 *		Constant Pools:
	 *			LONG (8 bytes) Pool [Indexed]:
	 *				...
	 *			String Pool [Indexed]:
	 *				...
	 *	Link Section
	 *		Usings Segment [Indexed?]:
	 *			Name
	 *		Used Types Segment [Indexed]: {NOTE: When an instruction references a type, a negative index implies it is a locally defined type, listed in Defined Type Ids
	 *			Index of containing file + 1 (0 is system)
	 *			Name
	 *			Type of type [enum...]
	 *			Number of generics
	 *			generics[number of generics]:
	 *				index of generic parameter in used types segment.
	 *			...
	 *			{During loading, the values in this are resolved to TypeIds}
	 *		Id Strings segment [Indexed]:
	 *			...
	 *	Info Section
	 *		Defined Type Ids [Indexed]
	 *			Name of type
	 *		Non-Local Procedure Stubs:
	 *			Index of Type in used types segment ???
	 *			Name
	 *		Exported Types Segment:
	 *			Index of entry in used types segment...?
	 *			...
	 *		Procedures Segment:
	 *			Index of name in id strings segment
	 *			stack size
	 *			number of parameters
	 *			parameters:
	 *				Name
	 *				Index of type [Type Index: A negative value indicates it is a locally defined type]
	 *				Usage info bit-flags?
	 *			index of return type
	 *			offset of first instruction in code segment
	 *			attributes:
	 *				???
	 *		Local Types Segment?:
	 *			...
	 *	Code Section
	 */
	internal struct UsedTypeDefinition
	{
		public readonly string Name;
		//public readonly ushort[] Generics;
		public readonly ushort IndexOfContainingFile;
		//public byte TypeOfType;

		public UsedTypeDefinition(string name, 
			//ushort[] generics, 
			ushort indexOfContainingFile
			//, byte typeOfType
			)
		{
			Name = name;
			//Generics = generics;
			IndexOfContainingFile = indexOfContainingFile;
			//TypeOfType = typeOfType;
		}
	}

	internal readonly struct UsedTypesCollection : IReadOnlyList<UsedTypeDefinition>
	{
		private readonly string[] Names;
		private readonly ushort[] IndexesOfContainingFiles;

		public int Count => Names.Length;

		public UsedTypeDefinition this[int index] => new UsedTypeDefinition(Names[index], IndexesOfContainingFiles[index]);

		public UsedTypesCollection(string[] names, ushort[] indexesOfContainingFiles)
		{
			if (indexesOfContainingFiles.Length != names.Length)
			{
				throw new ArgumentException();
			}
			Names = names;
			IndexesOfContainingFiles = indexesOfContainingFiles;
		}

		public IEnumerator<UsedTypeDefinition> GetEnumerator()
		{
			for (var i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

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

		// When compiling, file is null
		public ProcedureInfo(int offset, ushort stackSize, ushort numberOfParameters, LsnObjectFile file)
		{
			CodeOffset = offset; StackSize = stackSize;
			NumberOfParameters = numberOfParameters; File = file;
		}

		// ToDo: Serialize
	}

	/// <summary>
	/// Contains the information that the Virtual Machine needs to call a host interface method. Stored in the calling file.
	/// </summary>
	/// <remarks>
	/// If/when the VM transitions to using Handles for methods, types, & stuff, this will contain
	/// the method's handle in place of its name.
	/// </remarks>
	public readonly struct HostInterfaceSignatureStub : IEquatable<HostInterfaceSignatureStub>
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
		/// Initializes a new instance of the <see cref="HostInterfaceSignatureStub"/> struct.
		/// </summary>
		/// <param name="identifier">The identifier of the method.</param>
		/// <param name="numberOfParameters">The number of parameters the method takes, including 'self'.</param>
		/// <param name="hostInterfaceName"> The name of the host interface this method belongs to. </param>
		public HostInterfaceSignatureStub(string identifier, int numberOfParameters, string hostInterfaceName)
		{
			Identifier = identifier;
			NumberOfParameters = numberOfParameters;
			HostInterfaceName = hostInterfaceName;
		}

		/// <summary>Initializes a new instance of the <see cref="HostInterfaceSignatureStub" /> struct.</summary>
		/// <param name="signature">The function signature to base this stub on.</param>
		/// <param name="hostInterfaceName"> The name of the host interface this method belongs to. </param>
		public HostInterfaceSignatureStub(FunctionSignature signature, string hostInterfaceName) : this(signature.Name, signature.Parameters.Count, hostInterfaceName)
		{}

		/// <inheritdoc />
		/// <remarks>
		/// Does not take into account <see cref="NumberOfParameters"/>.
		/// A host interface cannot have two different methods with the same identifier.
		/// </remarks>
		public bool Equals(HostInterfaceSignatureStub other)
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
			return obj is HostInterfaceSignatureStub other && Equals(other);
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

	public sealed class LsnObjectFileHeader
	{
		public static LsnObjectFileHeader Read()
		{
			throw new NotImplementedException();
		}

		public void Write()
		{
			throw new NotImplementedException();
		}
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
		/// The files referenced by this file.
		/// </summary>
		private readonly LsnObjectFile[] ReferencedFiles;

		/// <summary>
		/// The referenced types
		/// </summary>
		private readonly TypeId[] ReferencedTypes;

		/// <summary>
		/// The defined types
		/// </summary>
		private readonly TypeId[] DefinedTypes;

		/// <summary>
		/// The identifier strings
		/// </summary>
		private readonly string[] IdentifierStrings;

		/// <summary>
		/// A lookup of type indexes in <see cref="DefinedTypes"/> by name.
		/// </summary>
		private readonly IReadOnlyDictionary<string, ushort> DefinedTypesIndexLookup;

		/// <summary>
		/// The signature stubs of host interface methods called by code in this file.
		/// </summary>
		private readonly HostInterfaceSignatureStub[] SignatureStubs;

		/// <summary>
		/// A lookup of procedure indexes in <see cref="ContainedProcedures"/> by procedure name.
		/// </summary>
		private readonly IReadOnlyDictionary<string, int> ProcedureIndexLookup;

		// ToDo: Defined types (local and exported stored separately)


		// ToDo Separate into local vs exported.
		/// <summary>
		/// The procedures contained in this file.
		/// </summary>
		private readonly ProcedureInfo[] ContainedProcedures;

		public LsnObjectFile(string filePath, double[] doubles, string[] strings, TypeId[] referencedTypes,
			TypeId[] definedTypes, string[] identifierStrings,
			IReadOnlyDictionary<string, ushort> definedTypesIndexLookup, HostInterfaceSignatureStub[] signatureStubs,
			IReadOnlyDictionary<string, int> procedureIndexLookup, ProcedureInfo[] containedProcedures,
			LsnObjectFile[] referencedFiles, Instruction[] code)
		{
			FilePath = filePath;
			ConstTable = new(doubles, strings);
			ReferencedTypes = referencedTypes;
			DefinedTypes = definedTypes;
			IdentifierStrings = identifierStrings;
			DefinedTypesIndexLookup = definedTypesIndexLookup;
			SignatureStubs = signatureStubs;
			ProcedureIndexLookup = procedureIndexLookup;
			ContainedProcedures = containedProcedures;
			ReferencedFiles = referencedFiles;
			Code = code;
		}

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
				for (var i = 0; i < strings.Length; i++)
				{
					ConstStrings[i] = new(strings[i]);
				}
			}

			internal double GetDouble(ushort index) => ConstDoubles[index];
			internal StringValue GetString(ushort index) => ConstStrings[index];

			internal void Write(BinaryStream writer)
			{
				writer.Write((ushort)ConstDoubles.Length);
				writer.Write(ConstDoubles);
				writer.Write((ushort)ConstStrings.Length);
				writer.Write(ConstStrings.Select(s => s.Value));
			}

			internal static ConstTableStruct Read(BinaryStream stream)
			{
				var constDoublesLength = stream.ReadUInt16();
				var constDoubles = stream.ReadDoubles(constDoublesLength);
				var constStringsLength = stream.ReadUInt16();

				var constStrings = stream.ReadStrings(constStringsLength);

				var lsnStrings = new StringValue[constStringsLength];
				for (int i = 0; i < constStringsLength; i++)
				{
					lsnStrings[i] = new(constStrings[i]);
				}

				return new(constDoubles, constStrings);
			}
		}

		/// <summary>
		/// Gets a string that is the name of something
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal string GetIdentifierString(ushort index) => IdentifierStrings[index];

		internal HostInterfaceSignatureStub GetSignatureStub(ushort index) => SignatureStubs[index];

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
		internal TypeId GetContainedType(string name) => DefinedTypes[DefinedTypesIndexLookup[name]];

		/// <summary>
		/// Get an LSN procedure contained in this file.
		/// </summary>
		/// <param name="index"></param>
		internal ProcedureInfo GetProcedure(ushort index) => ContainedProcedures[index];

		internal ProcedureInfo GetProcedure(string name) => ContainedProcedures[ProcedureIndexLookup[name]];

		internal LsnObjectFile GetFile(ushort index) => ReferencedFiles[index];
	}
}
