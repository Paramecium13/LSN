using LsnCore.Serialization;
using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public class GameValue
	{
		public readonly TypeId Type;
		public readonly string Name;

		public GameValue(TypeId type, string name)
		{
			Type = type; Name = name;
		}

		public void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			resourceSerializer.WriteTypeId(Type, writer);
			writer.Write(new string(Name.Skip(1).ToArray()));
		}

		public static GameValue Read(BinaryStream reader, ResourceDeserializer resourceDeserializer)
		{
			var type = resourceDeserializer.LoadTypeId(reader);
			var name = "$" + reader.ReadString();
			return new GameValue(type, name);
		}
	}
}
