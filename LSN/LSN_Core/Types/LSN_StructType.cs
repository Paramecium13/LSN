using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// This type repressents a struct type, it has strongly typed members, which are LSN_Values,
	/// that are accessed by name. It's instances are passed by value.
	/// </summary>
	public class LSN_StructType : LSN_Type
	{
		public Dictionary<String, LSN_Type> Members = new Dictionary<string, LSN_Type>();

		public override ILSN_Value CreateDefaultValue()
		{
			var dict = new Dictionary<string, ILSN_Value>();
			foreach(var pair in Members)
			{
				dict.Add(pair.Key, pair.Value.CreateDefaultValue());
			}
			return new StructValue(this, dict);
		}

		public override int GetSize()
		{
			int size = 0;
			foreach(var type in Members.Values)
			{
				size += type.GetSize();
			}
			return size;
		}
	}
}
