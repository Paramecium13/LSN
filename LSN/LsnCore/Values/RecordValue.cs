using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	[Serializable]
	public class RecordValue : LSN_Value, IHasFieldsValue
	{
		private RecordType _Type;
		public override bool BoolValue { get { return true; } }

		private Dictionary<string, ILsnValue> Values = new Dictionary<string, ILsnValue>();

		public RecordValue(RecordType type, Dictionary<string,ILsnValue> values)
		{
			Type = type; _Type = type;
			// Assume 'values' has already been type checked.
			foreach (var pair in values)
				Values.Add(pair.Key, pair.Value.Clone());
		}


		public RecordValue(RecordType type)
		{
			Type = type; _Type = type;
			foreach (var pair in _Type.Fields)
				Values.Add(pair.Key, pair.Value.CreateDefaultValue());
		}

		public override ILsnValue Clone() => new RecordValue(_Type, Values);


		public ILsnValue GetValue(string name) => Values[name]; 
		// This should be checked by the reifier.

		public void SetValue(string name, ILsnValue value)
		{
			Values[name] = value;
		}
	}
}
