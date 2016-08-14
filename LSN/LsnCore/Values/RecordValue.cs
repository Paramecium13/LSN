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
		private readonly RecordType _Type;
		public override LsnType Type { get { return _Type; } protected set {} }
		public override bool BoolValue { get { return true; } }

		private readonly ILsnValue[] Values;

		public RecordValue(RecordType type, IDictionary<string,ILsnValue> values)
		{
			_Type = type;
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
			if(clone)
			{
				int length = _Type.FieldCount;
				Values = new ILsnValue[length];
				for(int i = 0; i < length; i++)
					Values[i] = values[i].Clone();
			}
			else
				Values = values;
		}

		public override ILsnValue Clone() => new RecordValue(_Type, Values, true);


		public ILsnValue GetValue(string name) => Values[_Type.GetIndex(name)];
		// This should be checked by the reifier.

		public ILsnValue GetValue(int index) => Values[index];

		public void SetValue(string name, ILsnValue value)
		{
			Values[_Type.GetIndex(name)] = value;
		}

		public void SetValue(int index, ILsnValue value)
		{
			Values[index] = value;
		}
	}
}
