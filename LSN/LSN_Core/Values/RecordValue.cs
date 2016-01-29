using LSN_Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Values
{
	public class RecordValue : LSN_Value, IHasFieldsValue
	{
		private RecordType _Type;
		public override bool BoolValue { get { return true; } }

		private Dictionary<string, ILSN_Value> Values = new Dictionary<string, ILSN_Value>();

		public RecordValue(RecordType type, Dictionary<string,ILSN_Value> values)
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

		public override ILSN_Value Clone() => new RecordValue(_Type, Values);


		public ILSN_Value GetValue(string name) => Values[name]; 
		// This should be checked by the reifier.

		public void SetValue(string name, ILSN_Value value)
		{
			Values[name] = value;
		}

		public override string TranslateUniversal()
		{
			throw new NotImplementedException();
		}
	}
}
