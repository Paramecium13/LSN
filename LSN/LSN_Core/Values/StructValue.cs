using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;
using LSN_Core.Types;

namespace LSN_Core
{
	[Serializable]
	public struct StructValue : ILSN_Value, IHasFieldsValue
	{
		private readonly Dictionary<string, ILSN_Value> Members;

		private readonly LSN_StructType _Type;
		public LSN_Type Type => _Type;

		public bool BoolValue { get{ return true; } }

		public StructValue(LSN_StructType type, Dictionary<string, ILSN_Value> values)
		{
			_Type = type;
			Members = values ?? new Dictionary<string, ILSN_Value>();
			foreach(var pair in type.Fields)
			{
				if (!Members.ContainsKey(pair.Key))
					Members.Add(pair.Key, pair.Value.CreateDefaultValue());
			}
		}

		public ILSN_Value GetValue(string name) => Members[name];

		/// <summary>
		/// Create a new struct, 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public StructValue SetValue(string name, ILSN_Value value)
		{
			var dict = new Dictionary<string, ILSN_Value>();
			foreach(var pair in Members)
			{
				dict.Add(pair.Key, pair.Value);
			}
			dict[name] = value;
			return new StructValue((LSN_StructType)Type, dict);
		}

		public ILSN_Value Clone() => new StructValue(_Type, Members);

		public ILSN_Value DeepClone()
		{
			var dict = new Dictionary<string, ILSN_Value>();
			foreach (var pair in Members)
			{
				dict.Add(pair.Key, pair.Value);
			}
			return new StructValue((LSN_StructType)Type, dict);
		}

		public string TranslateUniversal()
		{
			throw new NotImplementedException();
		}

		public ILSN_Value Eval(IInterpreter i) => this;

		public IExpression Fold() => this;

		public bool IsReifyTimeConst() => true;

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
