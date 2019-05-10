using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore
{
	public class BoolType : LsnBoundedType<bool>
	{
		public static readonly BoolType Instance = new BoolType();
		BoolType():base("bool", ()=> LsnBoolValue.GetBoolValue(false)) { }

		public override LsnValue CreateDefaultValue() => LsnBoolValue.GetBoolValue(false);

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
		{
			setter(new LsnValue(reader.ReadBoolean()));
			return true;
		}

		internal override void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
			=> writer.Write(value.BoolValue);
	}

}
