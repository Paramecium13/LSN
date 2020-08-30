﻿using System;
using System.Collections.Generic;
using LsnCore.Types;
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
		public int Index;

		public override bool IsPure => true;

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
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.Variable);
			writer.Write((ushort)Index);
		}

		/// <inheritdoc/>
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc/>
		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			if (Variable != null) return Variable.AccessExpression.GetInstructions(context);
			if (Index != 0) throw new ApplicationException();

			return new [] {new SimplePreInstruction(OpCode.LoadLocal, 0)};

		}
	}
}
