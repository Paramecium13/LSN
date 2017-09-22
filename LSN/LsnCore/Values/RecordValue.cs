using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using LsnCore.Types;
using System.Linq;
using Syroot.BinaryData;

namespace LsnCore
{
	[Serializable]
	public class RecordValue : ILsnValue, IHasFieldsValue
	{
		private readonly LsnValue[] Fields;

		public bool IsPure => true;

		[NonSerialized]
		private readonly RecordType _Type;

		private readonly TypeId Id;
		public TypeId Type => Id;

		public bool BoolValue { get{ return true; } }

		public RecordValue(RecordType type, IDictionary<string, LsnValue> values)
		{
			_Type = type;
			Id = type.Id;
			Fields = new LsnValue[_Type.FieldCount];
			foreach(var pair in values)
			{
				Fields[_Type.GetIndex(pair.Key)] = pair.Value;
			}
		}

		public RecordValue(RecordType type, LsnValue[] values)
		{
			_Type = type; Id = type.Id; Fields = values;
		}

		public RecordValue(LsnValue[] values, TypeId id)
		{
			Id = id; Fields = values;
		}

		public RecordValue(LsnValue[] values)
		{
			Fields = values;
		}


		public LsnValue GetFieldValue(int index) => Fields[index];


		public ILsnValue Clone() => this;//new StructValue(_Type, Members);

		public ILsnValue DeepClone()
		{
			return new RecordValue(Fields.Select(f=>f.Clone()).ToArray());
		}
		
		public LsnValue Eval(IInterpreter i) => new LsnValue(this);

		public IExpression Fold() => this;

		public bool IsReifyTimeConst() => true;

		public void Replace(IExpression oldExpr, IExpression newExpr){}

		public bool Equals(IExpression other) => false;


		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write((byte)ConstantCode.Record);
			writer.Write((ushort)Fields.Length);
			for (int i = 0; i < Fields.Length; i++)
				Fields[i].Serialize(writer);
		}

		/*
		public static StructValue operator + (StructValue a, StructValue b)
		{

		}

		public static StructValue operator - (StructValue a, StructValue b)
		{

		}
		*/
	}
}
