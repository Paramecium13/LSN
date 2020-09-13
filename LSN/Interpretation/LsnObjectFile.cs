using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	/// <summary>
	/// Information about a procedure.
	/// </summary>
	public readonly struct ProcedureInfo
	{
		public readonly int CodeOffset;
		public readonly ushort StackSize;
		public readonly LsnObjectFile File;
		public readonly ushort NumberOfParameters;

		public ProcedureInfo(int offset, ushort stackSize, ushort numberOfParameters, LsnObjectFile file)
		{
			CodeOffset = offset; StackSize = stackSize;
			NumberOfParameters = numberOfParameters; File = file;
		}
	}

	public enum ProcedureClassification
	{
		Function,
		Method,
		Constructor,
		EventHandler,
	}

	public class ProcedureDefinition
	{
		public readonly string Name;
		public readonly ProcedureInfo Info;
		public readonly TypeId ReturnType;
		public readonly ProcedureClassification Classification;
	}
	
	/// <summary>
	/// 
	/// </summary>
	public class LsnObjectFile
	{
		private readonly ProcedureDefinition[] ProcedureDefinitions;

		/// <summary>
		/// Gets the name of this file.
		/// </summary>
		public string FileName { get; }

		internal Instruction[] Code { get; }
		
		internal LsnValue GetDouble(ushort index) => new LsnValue(ConstTable.GetDouble(index));

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
				ConstStrings = strings.Select(s => new StringValue(s)).ToArray();
			}

			internal double GetDouble(ushort index) => ConstDoubles[index];
			internal StringValue GetString(ushort index) => ConstStrings[index];
		}

		private readonly ConstTableStruct ConstTable;

		/// <summary>
		/// Gets a string that is the name of something
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal string GetIdentifierString(ushort index) => throw new NotImplementedException();

		/// <summary>
		/// Gets a type used by code in this file.
		/// </summary>
		internal LsnType GetUsedType(ushort index) => throw new NotImplementedException();

		/// <summary>
		/// Gets a type contained in this file.
		/// </summary>
		internal LsnType GetContainedType(string name) => throw new NotImplementedException();

		/// <summary>
		/// Get a procedure called by code in this file.
		/// </summary>
		/// <param name="index"></param>
		internal ProcedureInfo GetProcedure(ushort index) => throw new NotImplementedException();

		/// <summary>
		/// Gets a procedure contained in this file.
		/// </summary>
		/// <param name="name">The name.</param>
		internal ProcedureDefinition GetContainedProcedure(string name) =>
			ProcedureDefinitions.FirstOrDefault(p => p.Name == name);

		internal LsnObjectFile GetFile(ushort index) => throw new NotImplementedException();
	}

}
