using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Debug
{
	internal readonly struct InstructionLineInfo
	{
		internal readonly ushort InstructionIndex;
		internal readonly ushort LineNumber;
		internal InstructionLineInfo(ushort instrIndex, ushort linNo) { InstructionIndex = instrIndex; LineNumber = linNo; }
	}

	public class ProcedureLineInfo
	{
		readonly InstructionLineInfo[] LineInfo;
		/*	Invariant: Ordering of Instruction Indexes
		 *		For i, 0 <= i < LineInfo.Length - 1
		 *		LineInfo[i].InstructionIndex < LineInfo[i+1].InstructionIndex
		 *		
		 *	Finding the line number of instruction u:
		 *		Find some i, so that:
		 *			(a) LineInfo[i].InstructionIndex == u,
		 *			(b) i < LineInfo.Length - 1
		 *					&& LineInfo[i].InstructionIndex < u < LineInfo[i + 1].InstructionIndex,
		 *			or (c) i == LineInfo.Length - 1.
		 *		Return LineInfo[i].LineNumber
		 *			
		 */

		internal ushort GetLine(ushort instructionIndex) => throw new NotImplementedException();
	}
}
