using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Interpretation;
using LsnCore.Types;
using LSNr.Utilities;

namespace LSNr
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class ObjectFileHeaderFactory
	{
		private readonly TableBuilder<double> DoubleConstTable = new();

		private readonly TableBuilder<string> StringConstTable = new();

		private readonly TableBuilder<string> ReferencedFilesTable = new();

		/// <summary>
		/// The identifier table
		/// </summary>
		private readonly TableBuilder<string> IdentifierTable = new();

		/// <summary>
		/// The table of host interface method signatures.
		/// </summary>
		private readonly TableBuilder<SignatureStub> SignatureTable = new();

		private readonly TableBuilder<TypeId> ReferencedTypeTable = new();


		public string FilePath { get; }

		public ObjectFileHeaderFactory(string filePath)
		{
			FilePath = filePath;
		}

		/// <summary>
		/// Adds <paramref name="constant"/> to the constant table if it isn't already in the constant table.
		/// Returns the index of <paramref name="constant"/> in the table.
		/// </summary>
		/// <param name="constant">The constant.</param>
		/// <returns> The index of <paramref name="constant"/> in the constant table. </returns>
		public ushort AddConstant(double constant) => checked((ushort) DoubleConstTable.Add(constant));

		/// <summary>
		/// Adds <paramref name="constant"/> to the constant table if it isn't already in the constant table.
		/// Returns the index of <paramref name="constant"/> in the table.
		/// </summary>
		/// <param name="constant">The constant.</param>
		/// <returns> The index of <paramref name="constant"/> in the constant table. </returns>
		public ushort AddConstant(string constant) => checked((ushort) StringConstTable.Add(constant));

		public void AddFunctionReferenceByName(string filePath, string functionName, out ushort referencedFileIndex,
			out ushort nameIndex)
		{
			checked
			{
				referencedFileIndex = (ushort)ReferencedFilesTable.Add(filePath);
				nameIndex = (ushort) IdentifierTable.Add(functionName);
			}
		}

		/// <summary>
		/// Adds the host interface method signature to the signature table if it isn't already present.
		/// Returns the index of <paramref name="signatureStub"/> in the signature table.
		/// </summary>
		/// <param name="signatureStub">The signature stub.</param>
		/// <returns> The index of <paramref name="signatureStub"/> in the signature table. </returns>
		public ushort AddHostInterfaceMethodSignature(SignatureStub signatureStub) =>
			checked((ushort) SignatureTable.Add(signatureStub));

		/// <summary>
		/// Adds a referenced type.
		/// </summary>
		public short AddType(TypeId typeId) => checked((short) ReferencedTypeTable.Add(typeId));

		/// <summary>
		/// Adds an identifier string.
		/// </summary>
		/// <param name="identifierString"> The identifier string. </param>
		/// <returns></returns>
		public ushort AddIdentifierString(string identifierString) =>
			checked((ushort) IdentifierTable.Add(identifierString));
	}
}
