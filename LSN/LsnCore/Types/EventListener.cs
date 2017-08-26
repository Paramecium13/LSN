using LsnCore.Statements;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class EventListener
	{
		public readonly EventDefinition Definition;
		
		private readonly string ResourceFilePath;

		internal int StackSize;
		internal Statement[] Code;
		

		public EventListener(EventDefinition definition, string resourceFilePath)
		{
			Definition = definition; ResourceFilePath = resourceFilePath;
		}


		public void Run(LsnValue[] args, IInterpreter i)
		{
			i.Run(Code, ResourceFilePath, StackSize, args);
		}


		public void Serialize(BinaryDataWriter writer)
		{
			Definition.Serialize(writer);
			writer.Write((int)StackSize);
			new BinaryFormatter().Serialize(writer.BaseStream, Code);
		}


		public static EventListener Read(BinaryDataReader reader, ITypeIdContainer typeContainer, string resourceFilePath)
		{
			var def = EventDefinition.Read(reader, typeContainer);
			var stackSize = reader.ReadInt32();
			var code = (Statement[])new BinaryFormatter().Deserialize(reader.BaseStream);

			return new EventListener(def, resourceFilePath)
			{
				StackSize = stackSize,
				Code = code
			};
		}

	}
}
