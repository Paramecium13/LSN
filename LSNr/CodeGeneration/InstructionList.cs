using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.CodeGeneration
{
	/// <summary>
	/// Stores instructions from expressions and statements. Keeps track of the label to apply to the next instruction.
	/// </summary>
	public class InstructionList
	{
		private readonly List<PreInstruction> PreInstructions = new();

		private InstructionLabel NextLabel { get; set; }

		/// <summary>
		/// Sets the label to apply to the next instruction.
		/// </summary>
		/// <param name="label">The label.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void SetNextLabel(InstructionLabel label)
		{
			if (NextLabel != null)
			{
				throw new InvalidOperationException();
			}

			NextLabel = label;
		}

		/// <summary>
		/// Adds an instruction.
		/// </summary>
		/// <param name="instruction">The instruction.</param>
		public void AddInstruction(PreInstruction instruction)
		{
			if (NextLabel != null)
			{
				instruction.Labels.Add(NextLabel);
				NextLabel = null;
			}

			PreInstructions.Add(instruction);
		}

		/// <summary>
		/// Gets the pre instructions.
		/// </summary>
		/// <returns></returns>
		internal PreInstruction[] GetPreInstructions() => PreInstructions.ToArray();
	}
}
