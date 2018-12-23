using LsnCore.Expressions;
using LsnCore.Serialization;
using LsnCore.Statements;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LsnCore.Types
{
	public class ScriptClassMethod : Method, ICodeBlock, IProcedure
	{
		public readonly bool IsVirtual;

		public readonly bool IsAbstract;

		public Statement[] Code { get; set; } // Assigned in LSNr.

		public ScriptClassMethod(TypeId type, TypeId returnType, IList<Parameter> parameters, string resourceFilePath,
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

		public Expression CreateScriptObjectMethodCall(IExpression[] parameters)
		{
			if (parameters.Length != Parameters.Count)
				throw new ApplicationException();
			if (parameters[0].Type != TypeId)
				throw new ApplicationException();

			//if (IsVirtual)
				return new ScriptObjectVirtualMethodCall(parameters, Name, ReturnType);
			//else
				//return new MethodCall(this, parameters); Can't do this, would result in this method being serialized along with it's call.
		}

		public override IExpression CreateMethodCall(IList<Tuple<string, IExpression>> args, IExpression expression)
		{
			var argsArray = new IExpression[Parameters.Count];
			argsArray[0] = expression;
			var dict = args.ToDictionary(t => t.Item1, t => t.Item2);
			for (int i = 1; i < Parameters.Count; i++)
			{
				var p = Parameters[i];
				argsArray[i] = dict.ContainsKey(p.Name) ? dict[p.Name] : p.DefaultValue;
			}
			return CreateScriptObjectMethodCall(argsArray);
		}

#if CORE
		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
		{
			i.RunProcedure(this, args);
			return i.ReturnValue;
		}
#endif

		//enum Flags : byte { none = 0, IsVirtual = 1, IsAbstract = 2 }

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			Signature.Serialize(writer, resourceSerializer);
			byte b = 0;
			if (IsAbstract)
				b = 2;
			else if (IsVirtual)
				b = 1;
			writer.Write(b);
			if(!IsAbstract)
			{
				writer.Write((ushort)StackSize);
				var offset = writer.ReserveOffset();

				writer.Write((ushort)Code.Length);
				for (int i = 0; i < Code.Length; i++)
					Code[i].Serialize(writer, resourceSerializer);

				offset.Satisfy((int)writer.Position - (int)offset.Position -4);
			}
		}

		internal static ScriptClassMethod Read(BinaryDataReader reader, ITypeIdContainer typeContainer, TypeId type, string resourceFilePath, ResourceDeserializer resourceDeserializer)
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

			if (!isAbstract)
			{
				stackSize = reader.ReadUInt16();
				var codeSize = reader.ReadInt32();
				resourceDeserializer.RegisterCodeBlock(method, reader.ReadBytes(codeSize));
			}

			return method;
		}

		public Method ToVirtualMethod()
			=> new ScriptClassVirtualMethod(TypeId, ReturnType, Parameters.ToList(), ResourceFilePath, Name);
	}

	public class ScriptClassVirtualMethod : Method, IProcedure
	{
		public Statement[] Code { get; set; } // Assigned in LSNr.

		internal ScriptClassVirtualMethod(TypeId type, TypeId returnType, IList<Parameter> parameters, string resourceFilePath, string name)
			: base(type, returnType, name, parameters)
		{
			if (Parameters[0].Name != "self")
				throw new ApplicationException("");
			ResourceFilePath = resourceFilePath;
		}
#if CORE
		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
			=> (args[0].Value as ScriptObject).GetMethod(Name).Eval(args, i);
#endif
	}
}
