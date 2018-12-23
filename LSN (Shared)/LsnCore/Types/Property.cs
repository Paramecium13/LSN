using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class Property
	{
		public readonly string Name;
		public readonly TypeId Type;

		//public readonly LsnValue DefaultValue;
		public readonly string Metadata;

		public Property(string name, TypeId type, /*LsnValue defaultValue,*/ string metadata)
		{
			Name = name; Type = type; /*DefaultValue = defaultValue;*/ Metadata = metadata;
		}

		public Property(string name, TypeId type/*, LsnValue defaultValue*/):this(name,type,/*defaultValue,*/null) { }

		//public Property(string name, TypeId type, string metadata) : this(name, type, LsnValue.Nil, metadata) { }
		//public Property(string name, TypeId type) : this(name, type, LsnValue.Nil, null) { }

		public void Write(BinaryDataWriter writer)
		{
			writer.Write(Name);
			writer.Write(Type.Name);
			writer.Write(Metadata??"");
		}

		public static Property Read(BinaryDataReader reader, ITypeIdContainer typeContainer)
		{
			var name = reader.ReadString();
			var typeName = reader.ReadString();
			var metadata = reader.ReadString();
			if (metadata == "")
				metadata = null;

			return new Property(name, typeContainer.GetTypeId(typeName), metadata);
		}
	}
}
