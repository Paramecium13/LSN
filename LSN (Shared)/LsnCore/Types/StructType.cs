using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public class StructType : LsnType, IHasFieldsType
	{
		private readonly Field[] _Fields;
		public IReadOnlyCollection<Field> FieldsB => _Fields;

		public int FieldCount => _Fields.Length;

		public StructType(TypeId type, Tuple<string, TypeId>[] fields)
		{
			Name = type.Name;
			Id = type;
			int length = fields.Length;
			_Fields = new Field[length];
			for (int i = 0; i < length; i++)
				_Fields[i] = new Field(i, fields[i].Item1, fields[i].Item2);
			type.Load(this);
		}

		public override LsnValue CreateDefaultValue()
			=> new LsnValue(new StructValue(Id, FieldsB.Select(f => f.Type.Type.CreateDefaultValue()).ToArray()));

		public int GetIndex(string name)
		{
			if (FieldsB.Any(f => f.Name == name))
				return FieldsB.First(f => f.Name == name).Index;
			throw new ApplicationException($"The struct type {Name} does not have a field named {name}.");
		}

		public TypeId GetFieldType(int index) => _Fields[index].Type;
		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write(Name);
			writer.Write((ushort)FieldCount);

			foreach (var field in _Fields)
			{
				writer.Write(field.Name);
				writer.Write(field.Type.Name);
			}
		}

		public static StructType Read(BinaryDataReader reader, ITypeIdContainer typeContainer)
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

			return new StructType(typeContainer.GetTypeId(name), fields);
		}

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
		{
			var vals = deserializer.GetArray(FieldCount);
			var compl = true;
			for (int i = 0; i < FieldCount; i++)
			{
				var j = i;
				compl &= _Fields[i].Type.Type.LoadAsMember(deserializer, reader, (x) => vals[j] = x);
			}
			return compl;
		}
	}
}
