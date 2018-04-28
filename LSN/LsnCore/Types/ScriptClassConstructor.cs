using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Serialization;
using LsnCore.Statements;
using LsnCore.Values;

namespace LsnCore.Types
{
	public class ScriptClassConstructor: ICodeBlock
	{
		public Statement[] Code { get; set; }
		public int StackSize;
		private readonly string ResourceFilePath;
		public readonly Parameter[] Parameters;

		public ScriptClassConstructor(string resourceFilePath, Parameter[] parameters)
		{
			ResourceFilePath = resourceFilePath; Parameters = parameters;
		}

		public ScriptClassConstructor(Statement[] code, int stackSize, string resourceFilePath, Parameter[] parameters)
		{
			Code = code; StackSize = stackSize; ResourceFilePath = resourceFilePath; Parameters = parameters;
		}

		internal void Run(IInterpreter i, LsnValue[] args)
		{
			i.Run(Code, ResourceFilePath, StackSize, args);
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)Parameters.Length);
			for (int i = 0; i < Parameters.Length; i++)
				Parameters[i].Serialize(writer, resourceSerializer);

			writer.Write((ushort)StackSize);
			var offset = writer.ReserveOffset();
			writer.Write((ushort)Code.Length);
			for (int i = 0; i < Code.Length; i++)
				Code[i].Serialize(writer, resourceSerializer);
			offset.Satisfy((int)writer.Position - (int)offset.Position - 4);
		}

		public static ScriptClassConstructor Read(BinaryDataReader reader, string resourceFilePath, ResourceDeserializer resourceDeserializer)
		{
			var nParams = reader.ReadByte();
			var parameters = new Parameter[nParams];
			for (ushort i = 0; i < nParams; i++)
				parameters[i] = Parameter.Read(i, reader, resourceDeserializer);
			var stackSize = reader.ReadUInt16();
			var codeSize = reader.ReadInt32();

			var constructor = new ScriptClassConstructor(resourceFilePath, parameters)
			{
				StackSize = stackSize
			};
			resourceDeserializer.RegisterCodeBlock(constructor, reader.ReadBytes(codeSize));
			return constructor;
		}
	}
}
