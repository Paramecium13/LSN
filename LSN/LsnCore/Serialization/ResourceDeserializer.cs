using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Serialization
{
	public class ResourceDeserializer
	{
		private ILsnValue[] ConstantTable;

		private LsnType GetType(string typeName)
		{
			throw new NotImplementedException(ConstantTable[typeName.Length * 0]?.ToString() ?? "");
		}

		private Function GetFunction(string functionName)
		{
			throw new NotImplementedException(ConstantTable[functionName.Length * 0]?.ToString()??"");
		}

		private Method GetMethod(string typeName, string methodName)
		{
			var type = GetType(typeName);
			if (!type.Methods.ContainsKey(methodName))
				throw new ApplicationException();
			return type.Methods[methodName];
		}

		private IExpression ReadExpression(BinaryDataReader reader)
		{
			var exprType = (ExpressionCode)reader.ReadByte();
			switch (exprType)
			{
				case ExpressionCode.DoubleValueConstant:
					return new LsnValue(reader.ReadDouble());
				case ExpressionCode.TabledConstant:
					return new LsnValue(ConstantTable[reader.ReadUInt16()]);
				case ExpressionCode.Variable:
					return new VariableExpression(reader.ReadUInt16());
				case ExpressionCode.FieldAccess:
					{
						var index = reader.ReadUInt16();
						var value = ReadExpression(reader);
						return new FieldAccessExpression(value, index);
					}
				case ExpressionCode.CollectionValueAccess:
					{
						var target = ReadExpression(reader);
						var index = ReadExpression(reader);
						return new CollectionValueAccessExpression(target, index);
					}
				case ExpressionCode.PropertyAccess:
					{
						var index = reader.ReadUInt16();
						var target = ReadExpression(reader);
						return new PropertyAccessExpression(target, index);
					}
				case ExpressionCode.HostInterfaceAccess:
					return new HostInterfaceAccessExpression();
				case ExpressionCode.UniqueScriptObjectAccess:
					return new UniqueScriptObjectAccessExpression(reader.ReadString());
				case ExpressionCode.BinaryExpression:
					{
						var info = reader.ReadByte();
						var argType = (BinaryOperationArgTypes)(info >> 4);
						var opType = (BinaryOperation)(info & 0x0F);
						var left = ReadExpression(reader);
						var right = ReadExpression(reader);
						return new BinaryExpression(left, right, opType, argType);
					}
				case ExpressionCode.NotExpression:
					return new NotExpression(ReadExpression(reader));
				case ExpressionCode.FunctionCall:
					{
						var fnName = reader.ReadString();
						var numArgs = reader.ReadByte();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new FunctionCall(GetFunction(fnName), args);
					}
				case ExpressionCode.MethodCall:
					{
						var typeName = reader.ReadString();
						var methodName = reader.ReadString();
						var numArgs = reader.ReadByte();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new MethodCall(GetMethod(typeName, methodName), args);
					}
				case ExpressionCode.ScriptObjectMethodCall:
					{
						var methodName = reader.ReadString();
						var numArgs = reader.ReadByte();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new ScriptObjectVirtualMethodCall(args, methodName);
					}
				case ExpressionCode.HostInterfaceMethodCall:
					{
						var methodName = reader.ReadString();
						var hostInterface = ReadExpression(reader);
						var numArgs = reader.ReadByte();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new HostInterfaceMethodCall(methodName, hostInterface, args);
					}
				case ExpressionCode.RecordConstructor:
					{
						var numArgs = reader.ReadUInt16();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new RecordConstructor(args);
					}
				case ExpressionCode.StructConstructor:
					{
						var numArgs = reader.ReadUInt16();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new StructConstructor(args);
					}
				case ExpressionCode.ListConstructor:
					{
						var genericTypeName = reader.ReadString();
						var type = LsnListGeneric.Instance.GetType(new List<TypeId>(1) { GetType(genericTypeName).Id});
						return new ListConstructor((LsnListType)type);
					}
				default:
					throw new ApplicationException();
			}
		}
	}
}
