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
	/// Exit the innermost loop.
	/// </summary>
	public class BreakStatement : Statement
	{
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			throw new InvalidOperationException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}
	}

	/// <summary>
	/// Move on to the next iteration of the innermost loop.
	/// </summary>
	public sealed class NextStatement : Statement
	{
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			throw new InvalidOperationException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}
	}
}
