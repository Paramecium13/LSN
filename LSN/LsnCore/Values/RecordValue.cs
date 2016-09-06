using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	[Serializable]
	public class RecordValue : LsnValue, IHasFieldsValue
	{
		[NonSerialized]
		private readonly RecordType _Type;
		private TypeId Id;
		public override TypeId Type => Id;
		public override bool BoolValue { get { return true; } }

		private readonly ILsnValue[] Values;

		public RecordValue(RecordType type, IDictionary<string,ILsnValue> values)
		{
			_Type = type;
			Id = new TypeId(type);
			// Assume 'values' has already been type checked.
			Values = new ILsnValue[_Type.FieldCount];
			foreach (var pair in values)
			{
				int i = _Type.GetIndex(pair.Key);
				Values[i] = pair.Value;
			}
		}

		
		public RecordValue(RecordType type, ILsnValue[] values, bool clone = false)
		{
			_Type = type;
			Id = new TypeId(type);
			if (clone)
			{
				int length = _Type.FieldCount;
				Values = new ILsnValue[length];
				for(int i = 0; i < length; i++)
					Values[i] = values[i].Clone();
			}
			else
				Values = values;
		}


		public RecordValue(TypeId id, ILsnValue[] values, bool clone = false)
		{
			Id = id;
			if (clone)
			{
				int length = values.Length;
				Values = new ILsnValue[length];
				for (int i = 0; i < length; i++)
					Values[i] = values[i].Clone();
			}
			else
				Values = values;
		}

		public override ILsnValue Clone() => new RecordValue(Id, Values, true);


		public ILsnValue GetValue(int index) => Values[index];


		public void SetValue(int index, ILsnValue value)
		{
			Values[index] = value;
		}
	}
}
