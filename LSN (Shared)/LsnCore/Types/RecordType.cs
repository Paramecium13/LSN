using Syroot.BinaryData;
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
	public class RecordType : LsnType, IHasFieldsType
	{
		private readonly Dictionary<string, LsnType> _Fields = new Dictionary<string, LsnType>(); // TODO: Replace with TypeId?
		
		public IReadOnlyDictionary<string, LsnType> Fields => _Fields;

		private readonly Field[] _FieldsB;
		public IReadOnlyCollection<Field> FieldsB => _FieldsB;

		public int FieldCount => _FieldsB.Length;

		public RecordType(TypeId type, Tuple<string, TypeId>[] fields)
		{
			Name = type.Name;
			Id = type;
			var length = fields.Length;
			_FieldsB = new Field[length];
			for (int i = 0; i < length; i++)
				_FieldsB[i] = new Field(i, fields[i].Item1, fields[i].Item2);
			type.Load(this);
		}

		public override LsnValue CreateDefaultValue()
			=> new LsnValue(new RecordValue(FieldsB.OrderBy(f => f.Index).Select(f => f.Type.Type.CreateDefaultValue()).ToArray(), Id));

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

		// ToDo: Make this use ResourceSerializer
		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write(Name);
			writer.Write((ushort)FieldCount);

			foreach (var field in _FieldsB)
			{
				writer.Write(field.Name);
				writer.Write(field.Type.Name);
			}
		}

		public static RecordType Read(BinaryDataReader reader, ITypeIdContainer typeContainer)
		{
			var name = reader.ReadString();
			var nFields = reader.ReadUInt16();
			var fields = new Tuple<string, TypeId>[nFields];
			for (int i = 0; i < nFields; i++)
			{
				var fName = reader.ReadString();
				var fTypeName = reader.ReadString();
				fields[i] = new Tuple<string, TypeId>(fName, typeContainer.GetTypeId(fTypeName));
			}

			return new RecordType(typeContainer.GetTypeId(name), fields);
		}
	}
}
