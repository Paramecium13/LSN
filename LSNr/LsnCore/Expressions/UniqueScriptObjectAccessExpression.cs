using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	/// <summary>
	/// An expression that returns the instance of a unique script object.
	/// </summary>
	/// <seealso cref="Expression" />
	public sealed class UniqueScriptObjectAccessExpression : Expression
	{
		/// <inheritdoc />
		public override bool IsPure => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="UniqueScriptObjectAccessExpression"/> class.
		/// </summary>
		/// <param name="type">The type.</param>
		public UniqueScriptObjectAccessExpression(TypeId type)
		{
			Type = type;
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			instructions.AddInstruction(new TypeTargetedInstruction(OpCode.Load_UniqueScriptClass, Type));
		}

		public override IExpression Fold() => this;

		/// <inheritdoc />
		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.UniqueScriptObjectAccess);
			resourceSerializer.WriteTypeId(Type, writer);
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}
	}
}
