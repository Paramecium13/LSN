using LsnCore.Serialization;
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
	internal interface ICodeBlock
	{
		Statement[] Code { set; }
	}

	[Serializable]
	public sealed class EventListener : ICodeBlock
	{
		public readonly EventDefinition Definition;

		private readonly string ResourceFilePath;

		internal int StackSize;
		public Statement[] Code { get; set; }

		public EventListener(EventDefinition definition, string resourceFilePath)
		{
			Definition = definition; ResourceFilePath = resourceFilePath;
		}

		public void Run(LsnValue[] args, IInterpreter i)
		{
			i.Run(Code, ResourceFilePath, StackSize, args);
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			Definition.Serialize(writer, resourceSerializer);
			writer.Write((ushort)StackSize);
			var offset = writer.ReserveOffset();
			writer.Write((ushort)Code.Length);
			for (int i = 0; i < Code.Length; i++)
				Code[i].Serialize(writer, resourceSerializer);
			offset.Satisfy((int)writer.Position - (int)offset.Position -4);
		}

		public static EventListener Read(BinaryDataReader reader, ITypeIdContainer typeContainer, string resourceFilePath, ResourceDeserializer resourceDeserializer)
		{
			var def = EventDefinition.Read(reader, typeContainer);
			var stackSize = reader.ReadUInt16();
			var codeSize = reader.ReadInt32();

			var listener = new EventListener(def, resourceFilePath)
			{
				StackSize = stackSize
			};
			resourceDeserializer.RegisterCodeBlock(listener, reader.ReadBytes(codeSize));
			return listener;
		}
	}
}
