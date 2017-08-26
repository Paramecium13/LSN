using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// A function written in LSN.
	/// </summary>
	[Serializable]
	public class LsnFunction : Function
	{
		/// <summary>
		/// This should only be set from within LSNr, where function bodies are parsed.
		/// </summary>
		public Statement[] Code;
		

		public override bool HandlesScope { get { return true; } }


		public LsnFunction(List<Parameter> parameters, LsnType returnType, string name,string resourceFilePath)
			:base(new FunctionSignature(parameters, name, returnType?.Id))
		{
			ResourceFilePath = resourceFilePath;
		}

		public LsnFunction(List<Parameter> parameters, TypeId returnType, string name, string resourceFilePath)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			ResourceFilePath = resourceFilePath;
		}


		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
		{
			i.Run(Code, ResourceFilePath, StackSize, args);
			i.ExitFunctionScope();
			return i.ReturnValue;
		}


		public void Serialize(BinaryDataWriter writer)
		{
			Signature.Serialize(writer);
			writer.Write(StackSize);
			new BinaryFormatter().Serialize(writer.BaseStream, Code);
		}

		public static LsnFunction Read(BinaryDataReader reader, ITypeIdContainer typeContainer, string resourceFilePath)
		{
			var signiture = FunctionSignature.Read(reader, typeContainer);
			var stackSize = reader.ReadInt32();
			var code = (Statement[])new BinaryFormatter().Deserialize(reader.BaseStream);

			return new LsnFunction(signiture.Parameters.ToList(), signiture.ReturnType, signiture.Name, resourceFilePath)
			{
				Code = code,
				StackSize = stackSize
			};
		}
    }
}
