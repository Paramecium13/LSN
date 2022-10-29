using System;
using System.Collections.Generic;
using LsnCore.Types;
using LSNr.CodeGeneration;
#if LSNR
using LSNr;
#endif
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	/// <summary>
	/// An expression that returns the value of a local variable
	/// </summary>
	/// <seealso cref="Expression" />
	public sealed class VariableExpression : Expression
	{
		/// <summary>
		/// The index of the variable
		/// </summary>
		public int Index;

		public override bool IsPure => true;

		/// <summary>
		/// The variable
		/// </summary>
		public readonly Variable Variable;

		public VariableExpression(int index, TypeId type, Variable variable)
		{
			Index = index;
			Type = type;
			Variable = variable;
		}

		public VariableExpression(int index)
		{
			Index = index;
		}

		/// <inheritdoc/>
		public override IExpression Fold() => this;

		/// <inheritdoc/>
		public override bool IsReifyTimeConst() => false;

		/// <inheritdoc/>
		public override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.Variable);
			writer.Write((ushort)Index);
		}

		/// <inheritdoc/>
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			instructions.AddInstruction(new LoadVariablePreInstruction(Variable));
		}
	}
}
