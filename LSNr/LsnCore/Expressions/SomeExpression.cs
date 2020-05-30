using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	internal sealed class SomeExpression : Expression
	{
		private IExpression Contents;

		internal SomeExpression(IExpression contents)
		{
			Contents = contents;
			Type = OptionGeneric.Instance.GetType(new TypeId[] { Contents.Type }).Id;
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

		public override bool IsReifyTimeConst()
			=> Contents.IsReifyTimeConst();

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			Contents.Serialize(writer, resourceSerializer);
		}
	}
}
