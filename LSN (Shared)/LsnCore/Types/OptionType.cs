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

	public sealed class OptionType : LsnType
	{
		public readonly TypeId Contents;

		public OptionType(TypeId contents)
		{
			Contents = contents;
			Id = new TypeId("Option`" + contents.Name);
		}

		public override LsnValue CreateDefaultValue() => LsnValue.Nil;

		public override bool Subsumes(LsnType type) =>
			type == NullType.Instance || (Contents?.Type?.Subsumes(type) ?? false) ? true : base.Subsumes(type);
	}

	public sealed class OptionGeneric : GenericType
	{
		public static readonly OptionGeneric Instance = new OptionGeneric();

		public override string Name => "Option";

		protected override LsnType CreateType(TypeId[] types)
		{
			if (types.Length != 1)
				throw new ArgumentException("Option types must have exactly one generic parameter.");
			return new OptionType(types[0]);
		}
	}
}
