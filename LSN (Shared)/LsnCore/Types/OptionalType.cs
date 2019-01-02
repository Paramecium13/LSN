using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Types
{
	public sealed class NullType : LsnType
	{
		public static readonly NullType Instance = new NullType();

		private NullType()
		{
			Name = "Null\0";
			Id = new TypeId(this);
		}

		public override LsnValue CreateDefaultValue()
			=> LsnValue.Nil;
	}

	public sealed class OptionalType : LsnType
	{
		public readonly TypeId Contents;

		public OptionalType(TypeId contents)
		{
			Contents = contents;
			Id = new TypeId("Optional`" + contents.Name);
		}

		public override LsnValue CreateDefaultValue() => LsnValue.Nil;

		public override bool Subsumes(LsnType type) =>
			type == NullType.Instance || Contents.Type.Subsumes(type) ? true : base.Subsumes(type);
	}

	public sealed class OptionalGeneric : GenericType
	{
		public static readonly OptionalGeneric Instance = new OptionalGeneric();

		public override string Name => "Optional";

		protected override LsnType CreateType(TypeId[] types)
		{
			if (types.Length != 1)
				throw new ArgumentException("Optional types must have exactly one generic parameter.");
			return new OptionalType(types[0]);
		}
	}
}
