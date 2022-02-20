using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using LsnCore.Serialization;

namespace LsnCore
{
	public interface ICompileTimeProcedure
	{
		Statement[] Code { get; set; }

		Instruction[] Instructions { get; set; }

		IReadOnlyList<Parameter> Parameters { get; }

		TypeId ReturnType { get; }

		string Name { get; }

		int StackSize { get; set; }

	}

	/// <summary>
	/// A function written in LSN.
	/// </summary>
	public class LsnFunction : Function, ICompileTimeProcedure//, ICodeBlock, IProcedure
	{
		/// <summary>
		/// This should only be set from within LSNr, where function bodies are parsed.
		/// </summary>
		public Statement[] Code { get; set; }

		public Instruction[] Instructions { get; set; }

		public LsnFunction(IReadOnlyList<Parameter> parameters, TypeId returnType, string name, string resourceFilePath)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			ResourceFilePath = resourceFilePath;
		}

#if CORE
		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
		{
			i.RunProcedure(this, args);
			return i.ReturnValue;
		}
#endif

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			Signature.Serialize(writer, resourceSerializer);
			writer.Write((ushort)StackSize);
			var offset = writer.ReserveOffset();
			writer.Write((ushort)Code.Length);
			for (int i = 0; i < Code.Length; i++)
				Code[i].Serialize(writer, resourceSerializer);
			offset.Satisfy((int)writer.Position - (int)offset.Position -4);
		}

		public static LsnFunction Read(BinaryDataReader reader, ITypeIdContainer typeContainer, string resourceFilePath, ResourceDeserializer resourceDeserializer)
		{
			var signature = FunctionSignature.Read(reader, typeContainer);
			var stackSize = reader.ReadUInt16();
			var codeSize = reader.ReadInt32();

			var fn = new LsnFunction(signature.Parameters.ToList(), signature.ReturnType, signature.Name, resourceFilePath)
			{
				StackSize = stackSize
			};
			resourceDeserializer.RegisterFunction(fn,reader.ReadBytes(codeSize));
			return fn;
		}
	}
}
