using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	public struct ProcedureInfo
	{
		public readonly int CodeOffset;
		public readonly ushort StackSize;
		public readonly LsnObjectFile File;

		public ProcedureInfo(int offset, ushort stackSize, LsnObjectFile file)
		{
			CodeOffset = offset; StackSize = stackSize; File = file;
		}
	}

	public class LsnObjectFile
	{
		public string FileName { get; }

		internal Instruction[] Code { get; }

		internal LsnValue GetInt(ushort index) => throw new NotImplementedException();

		internal LsnValue GetDouble(ushort index) => throw new NotImplementedException();

		internal LsnValue GetObject(ushort index) => throw new NotImplementedException();

		internal LsnType GetUsedType(ushort index) => throw new NotImplementedException();

		internal LsnType GetContainedType(string name) => throw new NotImplementedException();

		/// <summary>
		/// Get a procedure called by code in this file.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal ProcedureInfo GetProcedure(ushort index) => throw new NotImplementedException();

		internal IProcedureB GetContainedFunction(string name) => throw new NotImplementedException();

		internal LsnObjectFile GetFile(ushort index) => throw new NotImplementedException();

		/// <summary>
		/// Gets a string that is the name of something
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal string GetString(ushort index) => throw new NotImplementedException();
	}

}
