﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		private readonly TableBuilder<double> DoubleConstTable = new TableBuilder<double>();

		private readonly TableBuilder<string> StringConstTable = new TableBuilder<string>();

		private readonly TableBuilder<string> ReferencedFilesTable = new TableBuilder<string>();

		/// <summary>
		/// The identifier table
		/// </summary>
		private readonly TableBuilder<string> IdentifierTable = new TableBuilder<string>();

		/// <summary>
		/// The table of host interface method signatures.
		/// </summary>
		private readonly TableBuilder<SignatureStub> SignatureTable = new TableBuilder<SignatureStub>();

		public string FilePath { get; }

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

		public short AddType(TypeId typeId)
		{
			throw new NotImplementedException();
		}
	}
}