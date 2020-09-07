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
	internal sealed class SomeExpression : Expression
	{
		private IExpression Contents;

		internal SomeExpression(IExpression contents)
		{
			Contents = contents;
			Type = OptionGeneric.Instance.GetType(new[] { Contents.Type }).Id;
		}

		/// <inheritdoc />
		public override bool IsPure => Contents.IsPure;

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			Contents.GetInstructions(instructions, context);
		}

		/// <inheritdoc />
		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			throw new NotImplementedException();
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
		public override bool IsReifyTimeConst()
			=> Contents.IsReifyTimeConst();

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			Contents.Serialize(writer, resourceSerializer);
		}
	}
}
