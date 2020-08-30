using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LSNr;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	internal class HiddenCastExpression : Expression
	{
		private IExpression Contents;

		internal HiddenCastExpression(IExpression contents, TypeId type)
		{
			Contents = contents; Type = type;
		}

		public override bool IsPure => Contents.IsPure;

		public override IExpression Fold()
		{
			Contents = Contents.Fold();
			return this;
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Contents;
			foreach (var expr in Contents.SelectMany(e => e))
				yield return expr;
		}

		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
			=> Contents.GetInstructions(context);

		public override bool IsReifyTimeConst() => Contents.IsReifyTimeConst();

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			Contents.Serialize(writer, resourceSerializer);
		}
	}
}
