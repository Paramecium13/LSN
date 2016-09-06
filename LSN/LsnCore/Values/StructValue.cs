using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LsnCore
{
	[Serializable]// ToDo: change to class, pass by ref...
	public class StructValue : ILsnValue, IHasFieldsValue
	{
		private readonly IDictionary<string, ILsnValue> Members;
		private readonly ILsnValue[] Fields;

		[NonSerialized]
		private readonly LsnStructType _Type;

		private readonly TypeId Id;
		public TypeId Type => Id;

		public bool BoolValue { get{ return true; } }

		public StructValue(LsnStructType type, IDictionary<string, ILsnValue> values)
		{
			_Type = type;
			Id = new TypeId(type);
			Members = values ?? new Dictionary<string, ILsnValue>();
			foreach(var pair in type.Fields)
			{
				if (!Members.ContainsKey(pair.Key))
					Members.Add(pair.Key, pair.Value.CreateDefaultValue());
			}
		}

		public StructValue(LsnStructType type, ILsnValue[] values)
		{
			_Type = type; Id = new TypeId(type); Fields = values;
		}


		public StructValue(ILsnValue[] values, TypeId id)
		{
			Id = id; Fields = values;
		}
		

		public ILsnValue GetValue(int index) => Fields[index];

		/// <summary>
		/// Create a new struct, 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public StructValue SetValue(string name, ILsnValue value)
		{
			var dict = new Dictionary<string, ILsnValue>();
			foreach(var pair in Members)
			{
				dict.Add(pair.Key, pair.Value);
			}
			dict[name] = value;
			return new StructValue(_Type, dict);
		}

		public ILsnValue Clone() => this;//new StructValue(_Type, Members);

		public ILsnValue DeepClone()
		{
			var dict = new Dictionary<string, ILsnValue>();
			foreach (var pair in Members)
			{
				dict.Add(pair.Key, pair.Value);
			}
			return new StructValue(_Type, dict);
		}
		
		public ILsnValue Eval(IInterpreter i) => this;

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
