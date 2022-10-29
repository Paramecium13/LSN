using LsnCore.Serialization;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LsnCore.Runtime.Types
{
	public class ScriptClassMethod : Method, ICodeBlock, IProcedure
	{
		public readonly bool IsVirtual;

		public readonly bool IsAbstract;

		public Instruction[] Code { get; set; } // Assigned in LSNr.

		public ScriptClassMethod(TypeId type, TypeId returnType, IReadOnlyList<Parameter> parameters, string resourceFilePath,
			bool isVirtual, bool isAbstract, string name)
			:base(type,returnType,name,parameters)
		{
			if(Parameters[0].Name != "self")
				throw new ApplicationException("");
			ResourceFilePath = resourceFilePath;
			IsVirtual = isVirtual;
			IsAbstract = isAbstract;
			if (IsAbstract && !IsVirtual) throw new ArgumentException();
		}

		//enum Flags : byte { none = 0, IsVirtual = 1, IsAbstract = 2 }
		
		/*public void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			Signature.Serialize(writer, resourceSerializer);
			byte b = 0;
			if (IsAbstract)
				b = 2;
			else if (IsVirtual)
				b = 1;
			writer.Write(b);
			if (IsAbstract) return;
			writer.Write((ushort)StackSize);
			var offset = writer.ReserveOffset();

			writer.Write((ushort)Code.Length);
			foreach (var statement in Code)
				statement.Serialize(writer, resourceSerializer);

			offset.Satisfy((int)writer.Position - (int)offset.Position -4);
		}

		internal static ScriptClassMethod Read(BinaryStream reader, ITypeIdContainer typeContainer, TypeId type, string resourceFilePath, ResourceDeserializer resourceDeserializer)
		{
			var signature = FunctionSignature.Read(reader, typeContainer);
			var b = reader.ReadByte();
			var isVirtual = b > 0;
			var isAbstract = b == 2;
			var stackSize = -1;
			var method = new ScriptClassMethod(type, signature.ReturnType, signature.Parameters.ToList(), resourceFilePath, isVirtual, isAbstract, signature.Name)
			{
				StackSize = stackSize
			};

			if (isAbstract) return method;
			stackSize = reader.ReadUInt16();
			var codeSize = reader.ReadInt32();
			resourceDeserializer.RegisterCodeBlock(method, reader.ReadBytes(codeSize));

			method.StackSize = stackSize;
			return method;
		}*/

		public Method ToVirtualMethod()
			=> new ScriptClassVirtualMethod(TypeId, ReturnType, Parameters.ToList(), ResourceFilePath, Name);
	}

	public class ScriptClassVirtualMethod : Method, IProcedure
	{
		public Instruction[] Code { get; set; } // Assigned in LSNr.

		internal ScriptClassVirtualMethod(TypeId type, TypeId returnType, IReadOnlyList<Parameter> parameters, string resourceFilePath, string name)
			: base(type, returnType, name, parameters)
		{
			if (Parameters[0].Name != "self")
				throw new ApplicationException("");
			ResourceFilePath = resourceFilePath;
		}

		//public override LsnValue Eval(LsnValue[] args, IInterpreter i) => (args[0].Value as ScriptObject).GetMethod(Name).Eval(args, i);

	}
}
