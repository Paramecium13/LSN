using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Values;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	// ToDo: Use this!!!
	public class DetachStatement : Statement
	{
		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.Detach);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			instructionList.AddInstruction(new SimplePreInstruction(OpCode.Detach, 0));
		}
	}
}
