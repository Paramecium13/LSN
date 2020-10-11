using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Collections;
using LSNr;
using LSNr.CodeGeneration;

namespace LsnCore.Expressions
{
	/// <summary>
	/// Construct a <see cref="LsnCore.Values.LsnList"/>.
	/// </summary>
	/// <seealso cref="LsnCore.Expressions.IExpression" />
	public sealed class ListConstructor : IExpression
	{
		/// <inheritdoc/>
		public bool IsPure => false;

		/// <inheritdoc/>
		public TypeId Type { get; }

		private readonly TypeId GenericTypeId;

		public ListConstructor(LsnListType listType)
		{
			GenericTypeId = listType.GenericId;
			Type = listType.Id;
		}

		/// <inheritdoc />
		public bool Equals(IExpression other)
		{
			return this == other;
		}

		/// <inheritdoc />
		public IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			yield return new TypeTargetedInstruction(OpCode.ConstructList, Type);
		}

		/// <inheritdoc />
		public void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			instructions.AddInstruction(new TypeTargetedInstruction(OpCode.ConstructList, Type));
		}

		/// <inheritdoc />
		public IExpression Fold() => this;

		/// <inheritdoc />
		public bool IsReifyTimeConst() => false;

		/// <inheritdoc />
		public void Replace(IExpression oldExpr, IExpression newExpr)
		{}

		/// <inheritdoc />
		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.ListConstructor);
			resourceSerializer.WriteTypeId(GenericTypeId, writer);
		}

		/// <inheritdoc />
		public IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
