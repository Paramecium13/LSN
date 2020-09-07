using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	/// <summary>
	/// Display choices statement. Displays the registered choices to the user,
	/// sets $PC to the the instruction index corresponding to the user's selection,
	/// and clears the registered choices.
	/// </summary>
	/// <seealso cref="LsnCore.Statements.Statement" />
	public sealed class DisplayChoicesStatement : Statement
	{
		/// <inheritdoc />
		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		/// <inheritdoc />
		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.DisplayChoices);
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			instructionList.AddInstruction(new SimplePreInstruction(OpCode.CallChoices, 0));
		}

		protected override IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}
	}
}
