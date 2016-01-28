using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	[Serializable]
	public struct StructValue : ILSN_Value
	{
		private Dictionary<string, ILSN_Value> Members;

		public LSN_Type Type { get; set; }

		public bool BoolValue { get{ return true; } }

		public StructValue(LSN_StructType type, Dictionary<string, ILSN_Value> values)
		{
			Members = new Dictionary<string, ILSN_Value>();
			Type = type;
			foreach(var pair in type.Members)
			{
				Members.Add(pair.Key, pair.Value.CreateDefaultValue());
			}
			if (values == null) return;
			foreach(var pair in values)
			{
				Members[pair.Key] = pair.Value;
			}
		}

		public ILSN_Value GetValue(string name) => Members[name];

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

		public ILSN_Value Clone()
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

		public ILSN_Value Eval(IInterpreter i)
		{
			return this;
		}

		public IExpression Fold()
		{
			return null;
		}

		public bool IsReifyTimeConst()
		{
			return true;
		}

		public int GetSize()
		{
			return Type.GetSize();
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
