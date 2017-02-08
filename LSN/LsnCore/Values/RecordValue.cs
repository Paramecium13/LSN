using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	[Serializable]
	public class RecordValue : LsnValueB, IHasFieldsValue
	{
		[NonSerialized]
		private readonly RecordType _Type;
		public override bool BoolValue { get { return true; } }

		private readonly LsnValue[] Values;

		public RecordValue(RecordType type, IDictionary<string,LsnValue> values)
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

		
		public RecordValue(RecordType type, LsnValue[] values, bool clone = false)
		{
			_Type = type;
			Type = type.Id;
			if (clone)
			{
				int length = _Type.FieldCount;
				Values = new LsnValue[length];
				for(int i = 0; i < length; i++)
					Values[i] = values[i].Clone();
			}
			else
				Values = values;
		}


		public RecordValue(TypeId id, LsnValue[] values, bool clone = false)
		{
			Type = id;
			if (clone)
			{
				int length = values.Length;
				Values = new LsnValue[length];
				for (int i = 0; i < length; i++)
					Values[i] = values[i].Clone();
			}
			else
				Values = values;
		}
		

		public override ILsnValue Clone() => new RecordValue(Type, Values.Select(v=>v.Clone()).ToArray(), true);


		public LsnValue GetValue(int index) => Values[index];


		public void SetValue(int index, LsnValue value)
		{
			Values[index] = value;
		}
	}
}
