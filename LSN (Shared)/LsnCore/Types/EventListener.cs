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

	public interface IProcedure
	{
#if LSNR
		Statement[] Code { get; set; }
		int StackSize { get; set; }
#else
		Statement[] Code { get; }
		int StackSize { get; }
#endif
		string ResourceFilePath { get; }
	}

	public sealed class EventListener : ICodeBlock, IProcedure
	{
		public readonly EventDefinition Definition;

		public string ResourceFilePath { get; }

		public int StackSize { get; set; }
		public Statement[] Code { get; set; }

		public readonly int Priority;

		public EventListener(EventDefinition definition, string resourceFilePath, int priority=0)
		{
			Definition = definition; ResourceFilePath = resourceFilePath; Priority = priority;
		}

#if CORE
		public void Run(LsnValue[] args, IInterpreter i)
		{
			i.RunProcedure(this, args);
		}
#endif

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
