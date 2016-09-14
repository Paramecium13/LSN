using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public class RecordType : LsnType, IHasFieldsType
	{
		private readonly Field[] _FieldsB;
		public IReadOnlyCollection<Field> FieldsB => _FieldsB;

		public int FieldCount => _FieldsB.Length;

		public RecordType(string name, Dictionary<string,LsnType> fields)
		{
			Name = name;
			_FieldsB = new Field[fields.Count];
			int i = 0;
			foreach (var pair in fields)
			{
				_FieldsB[i] = new Field(i++, pair.Key, pair.Value);
			};
		}

		public override ILsnValue CreateDefaultValue()
			=> new RecordValue(this, FieldsB.Select(f => f.Type.Type.CreateDefaultValue()).ToArray());
		

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


		public TypeId GetFieldType(int index) => _FieldsB[index].Type;
	}
}
