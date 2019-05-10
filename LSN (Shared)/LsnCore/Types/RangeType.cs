using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Text;
using Syroot.BinaryData;

namespace LsnCore.Types
{
	public class RangeType : LsnType, IHasFieldsType
	{
		public static readonly RangeType Instance = new RangeType();

		public IReadOnlyCollection<Field> FieldsB { get; }
			= new List<Field>() { new Field(0,"Start",Int_), new Field(1,"End",Int_)};

		public override LsnValue CreateDefaultValue() => new LsnValue(new Values.RangeValue(0, 0));

		public int GetIndex(string name)
		{
			switch (name)
			{
				case "Start": return 0;
				case "End": return 1;
				default:
					throw new InvalidOperationException();
			}
		}

		RangeType()
		{
			Name = "Range";
			Id = new TypeId(this);
		}

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
		{
			setter(new LsnValue(new RangeValue(reader.ReadInt32(), reader.ReadInt32())));
			return true;
		}

		internal override void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
		{
			var val = value.Value as RangeValue;
			writer.Write(val.Start);
			writer.Write(val.End);
		}
	}
}
