using LsnCore.Expressions;
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
	public class ScriptObjectMethod : Method
	{
		public override bool HandlesScope => false;

		public readonly bool IsVirtual;


		public readonly bool IsAbstract;


		internal Statement[] Code; // Assigned in LSNr.



		public ScriptObjectMethod(TypeId type, TypeId returnType, IList<Parameter> parameters, string resourceFilePath,
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

		public override IExpression CreateMethodCall(IList<Tuple<string, IExpression>> args, IExpression expression, bool included = false)
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

		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
		{
			i.Run(Code, ResourceFilePath, StackSize, args);
			i.ExitFunctionScope();
			return i.ReturnValue;
		}


		//enum Flags : byte { none = 0, IsVirtual = 1, IsAbstract = 2 }

		public void Serialize(BinaryDataWriter writer)
		{
			Signature.Serialize(writer);
			byte b = 0;
			if (IsAbstract)
				b = 2;
			else if (IsVirtual)
				b = 1;
			writer.Write(b);
			if(!IsAbstract)
			{
				writer.Write(StackSize);
				new BinaryFormatter().Serialize(writer.BaseStream, Code);
			}
		}


		public static ScriptObjectMethod Read(BinaryDataReader reader, ITypeIdContainer typeContainer, TypeId type, string resourceFilePath)
		{
			var signature = FunctionSignature.Read(reader, typeContainer);
			var b = reader.ReadByte();
			var isVirtual = b > 0;
			var isAbstract = b == 2;
			Statement[] code = null;
			int stackSize = -1;
			if (!isAbstract)
			{
				stackSize = reader.ReadInt32();
				code = (Statement[])new BinaryFormatter().Deserialize(reader.BaseStream);
			}

			return new ScriptObjectMethod(type, signature.ReturnType, signature.Parameters.ToList(), resourceFilePath, isVirtual, isAbstract, signature.Name)
			{
				StackSize = stackSize
			};
		}

	}
}
