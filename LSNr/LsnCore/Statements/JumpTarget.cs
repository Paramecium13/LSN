using System;
using System.Collections.Generic;
using System.Linq;
using LsnCore.Expressions;
using LSNr.CodeGeneration;
#if LSNR
using LSNr;
#endif
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	/// <summary>
	/// Jump to the instruction at the index currently in the target register.
	/// </summary>
	/// <seealso cref="Statement"/>
	public class JumpToTargetStatement : Statement
	{
		/// <inheritdoc/>
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}

		/// <inheritdoc/>
		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		public override IEnumerator<IExpression> GetEnumerator()// => Enumerable.Empty<IExpression>().GetEnumerator();
		{
			yield break;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			instructionList.AddInstruction(new SimplePreInstruction(OpCode.JumpToTarget, 0));
		}

		/// <inheritdoc/>
		protected virtual IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context)
		{
			yield return new SimplePreInstruction(OpCode.JumpToTarget, 0);
		}
	}

	/// <summary>
	/// Set the target register.
	/// </summary>
	/// <seealso cref="Statement" />
	/// <seealso cref="IHasTargetStatement" />
	public sealed class SetTargetStatement : Statement, IHasTargetStatement
	{
		/// <summary>
		/// Gets or sets the target.
		/// </summary>
		public int Target { get; set; }

		/// <inheritdoc/>
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}

		/// <inheritdoc/>
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break; // ????
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			instructionList.AddInstruction(new TargetedPreInstruction(OpCode.SetTarget, target, context.LabelFactory));
		}

		/// <inheritdoc/>
		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			throw new NotSupportedException();
		}
	}
}