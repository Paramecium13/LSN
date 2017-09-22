using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	[Serializable]
	public class StructValue : LsnValueB, IHasMutableFieldsValue
	{
		[NonSerialized]
		private readonly StructType _Type;
		public override bool BoolValue { get { return true; } }

		private readonly LsnValue[] Values;

		public StructValue(StructType type, IDictionary<string,LsnValue> values)
		{
			_Type = type;
			Type = type.Id;
			// Assume 'values' has already been type checked.
			Values = new LsnValue[_Type.FieldCount];
			foreach (var pair in values)
			{
				int i = _Type.GetIndex(pair.Key);
				Values[i] = pair.Value;
			}
		}

		
		public StructValue(StructType type, LsnValue[] values)
		{
			_Type = type;
			Type = type.Id;
			int length = _Type.FieldCount;
			Values = new LsnValue[length];
			for(int i = 0; i < length; i++)
				Values[i] = values[i].Clone();
		}

		public StructValue(TypeId id, LsnValue[] values)
		{
			Type = id;
			int length = values.Length;
			Values = new LsnValue[length];
			for (int i = 0; i < length; i++)
				Values[i] = values[i].Clone();
		}

		public StructValue(LsnValue[] values)
		{
			int length = values.Length;
			Values = new LsnValue[length];
			for (int i = 0; i < length; i++)
				Values[i] = values[i].Clone();
		}


		public override ILsnValue Clone() => new StructValue(Type, Values.Select(v=>v.Clone()).ToArray());


		public LsnValue GetFieldValue(int index) => Values[index];


		public void SetFieldValue(int index, LsnValue value)
		{
			Values[index] = value;
		}
	}
}
