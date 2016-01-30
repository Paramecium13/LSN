using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core.Types
{
	/// <summary>
	/// This type repressents a struct type, it has strongly typed members, which are LSN_Values,
	/// that are accessed by name. It's instances are passed by value.
	/// </summary>
	[Serializable]
	public class LSN_StructType : LSN_Type, IHasFieldsType
	{
		private Dictionary<string, LSN_Type> _Fields = new Dictionary<string, LSN_Type>();
		public IReadOnlyDictionary<string, LSN_Type> Fields { get { return _Fields; } }

		public override ILSN_Value CreateDefaultValue()
		{
			var dict = new Dictionary<string, ILSN_Value>();
			foreach(var pair in Fields)
			{
				dict.Add(pair.Key, pair.Value.CreateDefaultValue());
			}
			return new StructValue(this, dict);
		}


	}
}
