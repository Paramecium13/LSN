using LsnCore.Serialization;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;

namespace LsnCore.Runtime.Types
{
	internal interface ICodeBlock
	{
		Instruction[] Code { set; }
	}

	/// <summary>
	/// A runtime procedure.
	/// </summary>
	public interface IProcedure
	{

		Instruction[] Code { get; }
		
		int StackSize { get; }

		string ResourceFilePath { get; }
	}

	public sealed class EventListener : ICodeBlock, IProcedure
	{
		public readonly EventDefinition Definition;

		public string ResourceFilePath { get; }

		public int StackSize { get; set; }
		public Instruction[] Code { get; set; }

		public readonly int Priority;

		public EventListener(EventDefinition definition, string resourceFilePath, int priority=0)
		{
			Definition = definition; ResourceFilePath = resourceFilePath; Priority = priority;
		}

		/*public void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			Definition.Serialize(writer, resourceSerializer);
			writer.Write((ushort)StackSize);
			var offset = writer.ReserveOffset();
			writer.Write((ushort)Code.Length);
			for (int i = 0; i < Code.Length; i++)
				Code[i].Serialize(writer, resourceSerializer);
			offset.Satisfy((int)writer.Position - (int)offset.Position -4);
		}*/

		public static EventListener Read(BinaryStream reader, ITypeIdContainer typeContainer, string resourceFilePath, ResourceDeserializer resourceDeserializer)
		{
			throw new NotImplementedException();
			/*var def = EventDefinition.Read(reader, typeContainer);
			var stackSize = reader.ReadUInt16();
			var codeSize = reader.ReadInt32();

			var listener = new EventListener(def, resourceFilePath)
			{
				StackSize = stackSize
			};
			reader.ReadInt32s(codeSize);
			//resourceDeserializer.RegisterCodeBlock(listener, reader.ReadBytes(codeSize));
			return listener;*/
		}
	}
}
