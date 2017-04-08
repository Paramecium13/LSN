using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class Property
	{
		public readonly string Name;
		public readonly TypeId Type;

		public readonly LsnValue DefaultValue;
		public readonly string MetaData;

		public Property(string name, TypeId type, LsnValue defaultValue, string metaData)
		{
			Name = name; Type = type; DefaultValue = defaultValue; MetaData = metaData;
		}

		public Property(string name, TypeId type, LsnValue defaultValue):this(name,type,defaultValue,null) { }


		public Property(string name, TypeId type, string metadata) : this(name, type, LsnValue.Nil, metadata) { }


		public Property(string name, TypeId type) : this(name, type, LsnValue.Nil, null) { }
	}
}
