using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using LsnCore.Types;
using System.Linq;

namespace LsnCore
{
	[Serializable]
	public class StructValue : ILsnValue, IHasFieldsValue
	{
		private readonly LsnValue[] Fields;

		public bool IsPure => true;

		[NonSerialized]
		private readonly LsnStructType _Type;

		private readonly TypeId Id;
		public TypeId Type => Id;

		public bool BoolValue { get{ return true; } }

		public StructValue(LsnStructType type, IDictionary<string, LsnValue> values)
		{
			_Type = type;
			Id = type.Id;
			Fields = new LsnValue[_Type.FieldCount];
			foreach(var pair in values)
			{
				Fields[_Type.GetIndex(pair.Key)] = pair.Value;
			}
		}

		public StructValue(LsnStructType type, LsnValue[] values)
		{
			_Type = type; Id = type.Id; Fields = values;
		}


		public StructValue(LsnValue[] values, TypeId id)
		{
			Id = id; Fields = values;
		}
		

		public LsnValue GetFieldValue(int index) => Fields[index];


		public ILsnValue Clone() => this;//new StructValue(_Type, Members);

		public ILsnValue DeepClone()
		{
			return new StructValue(_Type, Fields.Select(f=>f.Clone()).ToArray());
		}
		
		public LsnValue Eval(IInterpreter i) => new LsnValue(this);

		public IExpression Fold() => this;

		public bool IsReifyTimeConst() => true;

		public void Replace(IExpression oldExpr, IExpression newExpr){}

		public bool Equals(IExpression other) => false;

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
