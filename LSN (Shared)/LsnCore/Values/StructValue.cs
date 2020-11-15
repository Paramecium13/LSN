using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Values
{
	public class StructValue : LsnValueB, IHasMutableFieldsValue
	{
		public override bool BoolValue { get { return true; } }

		internal readonly LsnValue[] Values;

		public StructValue(TypeId id, LsnValue[] values)
		{
			Type = id;
			Values = values;
			/*int length = values.Length;
			Values = new LsnValue[length];
			for (int i = 0; i < length; i++)
				Values[i] = values[i].Clone();*/
		}

		public StructValue(LsnValue[] values)
		{
			int length = values.Length;
			Values = new LsnValue[length];
			for (int i = 0; i < length; i++)
				Values[i] = values[i].Clone();
		}

		/// <inheritdoc />
		public override ILsnValue Clone() => new StructValue(Type, Values.Select(v=>v.Clone()).ToArray());

		/// <inheritdoc />
		public LsnValue GetFieldValue(int index) => Values[index];

		/// <inheritdoc />
		public void SetFieldValue(int index, LsnValue value)
		{
			Values[index] = value;
		}

		public override void Serialize(BinaryDataWriter writer)
		{
			writer.Write((byte)ConstantCode.Struct);
			writer.Write((ushort)Values.Length);
			for (int i = 0; i < Values.Length; i++)
				Values[i].Serialize(writer);
		}
	}
}
