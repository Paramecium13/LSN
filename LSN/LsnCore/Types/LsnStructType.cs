using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Types
{
	/// <summary>
	/// This type repressents a struct type, it has strongly typed members, which are LSN_Values,
	/// that are accessed by name. It's instances are passed by value.
	/// </summary>
	[Serializable]
	public class LsnStructType : LsnType, IHasFieldsType
	{
		private Dictionary<string, LsnType> _Fields = new Dictionary<string, LsnType>();
		public IReadOnlyDictionary<string, LsnType> Fields { get { return _Fields; } }

		public LsnStructType(string name, Dictionary<string, LsnType> fields)
		{
			Name = name; _Fields = fields;
		}

		public override ILsnValue CreateDefaultValue()
		{
			var dict = new Dictionary<string, ILsnValue>();
			foreach(var pair in Fields)
			{
				dict.Add(pair.Key, pair.Value.CreateDefaultValue());
			}
			return new StructValue(this, dict);
		}


	}
}
