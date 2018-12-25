using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Types
{
	public sealed class OptionalType : LsnType
	{
		public readonly TypeId Contents;

		public OptionalType(TypeId contents)
		{
			Contents = contents;
			Id = new TypeId("Optional`" + contents.Name);
		}

		public override LsnValue CreateDefaultValue() => LsnValue.Nil;

		public override bool Subsumes(LsnType type)
		{
			if (Contents.Type.Subsumes(type)) return true;
			return base.Subsumes(type);
		}
	}

	public sealed class OptionalGenericType : GenericType
	{
		public override string Name => "Optional";

		protected override LsnType CreateType(TypeId[] types)
		{
			throw new NotImplementedException();
		}
	}
}
