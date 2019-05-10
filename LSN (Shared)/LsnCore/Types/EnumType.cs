﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Types
{
	public class EnumType : LsnType
	{
		private readonly IReadOnlyDictionary<string, int> StoIDict;
		private readonly IReadOnlyDictionary<int, string> ItoSDict;
		private readonly int DefaultValue;

		public EnumType(string name, IEnumerable<KeyValuePair<string,int>> values, int defaultValue)
		{
			Id = new TypeId(name);
			DefaultValue = defaultValue;
			StoIDict = values.ToDictionary(p => p.Key, p => p.Value);
			ItoSDict = values.ToDictionary(p => p.Value, p => p.Key);

			_Methods.Add("GetName",new BoundedMethod(this, LsnType.String_, (a) => new LsnValue(new StringValue(ItoSDict[a[0].IntValue])), "GetName", null));
			_Methods.Add("ToInt", new BoundedMethod(this, LsnType.Int_, (a) => a[0], "ToInt"));
			_Methods.Add("GetValue", new BoundedMethod(this, LsnType.Int_, (a) => a[0], "GetValue"));
		}

		public override LsnValue CreateDefaultValue()
		{
			return new LsnValue(DefaultValue);
		}

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
		{
			setter(new LsnValue(reader.ReadInt32()));
			return true;
		}

		internal override void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
		{
			writer.Write(value.IntValue);
		}
	}
}
