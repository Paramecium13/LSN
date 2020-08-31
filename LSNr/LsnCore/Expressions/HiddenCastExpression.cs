using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	internal sealed class HiddenCastExpression : Expression
	{
		private IExpression Contents;

		internal HiddenCastExpression(IExpression contents, TypeId type)
		{
			Contents = contents; Type = type;
		}

		/// <inheritdoc />
		public override bool IsPure => Contents.IsPure;

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			Contents.GetInstructions(instructions, context);
		}

		/// <inheritdoc />
		public override IExpression Fold()
		{
			Contents = Contents.Fold();
			return this;
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Contents;
			foreach (var expr in Contents.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc />
		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
			=> Contents.GetInstructions(context);

		/// <inheritdoc />
		public override bool IsReifyTimeConst() => Contents.IsReifyTimeConst();

		/// <inheritdoc />
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			Contents.Serialize(writer, resourceSerializer);
		}
	}
}
