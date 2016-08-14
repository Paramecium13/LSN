using System;
using System.Collections.Generic;
using System.Linq;
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
		public IReadOnlyDictionary<string, LsnType> Fields => _Fields;

		private readonly Field[] _FieldsB;
		public IReadOnlyCollection<Field> FieldsB => _FieldsB;

		public int FieldCount => _FieldsB.Length;

		public LsnStructType(string name, Dictionary<string, LsnType> fields)
		{
			Name = name; _Fields = fields;
			_FieldsB = new Field[fields.Count];
			int i = 0;
			foreach(var pair in fields)
			{
				_FieldsB[i] = new Field(i++, pair.Key, pair.Value);
			}
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


		public int GetIndex(string name)
		{
			try
			{
				var field = FieldsB.First(f => f.Name == name);
				return field.Index;
			}
			catch (Exception)
			{
				throw new ApplicationException($"The struct type {Name} does not have a field named {name}.");
			}
		}


		public LsnType GetFieldType(int index) => _FieldsB[index].Type;

	}
}
