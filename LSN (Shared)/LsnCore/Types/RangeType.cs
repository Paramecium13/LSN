using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Types
{
	public class RangeType : LsnType, IHasFieldsType
	{
		public static readonly RangeType Instance = new RangeType();

		public IReadOnlyCollection<Field> FieldsB { get; }
			= new List<Field>() { new Field(0,"Start",int_), new Field(1,"End",int_)};

		public override LsnValue CreateDefaultValue() => new LsnValue(new Values.RangeValue(0, 0));

		public int GetIndex(string name)
		{
			return name switch
			{
				"Start" => 0,
				"End" => 1,
				_ => throw new InvalidOperationException()
			};
		}

		private RangeType()
		{
			Name = "Range";
			Id = new TypeId(this);
		}
	}
}
