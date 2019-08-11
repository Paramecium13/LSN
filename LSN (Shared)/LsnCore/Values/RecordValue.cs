using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using LsnCore.Types;
using System.Linq;
using Syroot.BinaryData;

namespace LsnCore
{
	public class RecordValue : ILsnValue, IHasFieldsValue
	{
		private readonly LsnValue[] Fields;

		public static bool IsPure => true;
		public TypeId Type { get; }

		public bool BoolValue { get{ return true; } }

		public RecordValue(LsnValue[] values, TypeId id)
		{
			Type = id; Fields = values;
		}

		public RecordValue(LsnValue[] values)
		{
			Fields = values;
		}

		public LsnValue GetFieldValue(int index) => Fields[index];

		public ILsnValue Clone() => this;

		public ILsnValue DeepClone()
		{
			return new RecordValue(Fields.Select(f=>f.Clone()).ToArray());
		}

		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write((byte)ConstantCode.Record);
			writer.Write((ushort)Fields.Length);
			for (int i = 0; i < Fields.Length; i++)
				Fields[i].Serialize(writer);
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.TabledConstant);
			writer.Write(resourceSerializer.TableConstant(this));
		}
	}
}
