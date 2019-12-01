using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Serialization
{
	public class ResourceDeserializer
	{
		private ILsnValue[] ConstantTable;

		private TypeId[] TypeIds;

		private readonly List<Tuple<ICodeBlock, byte[]>> CodeBlocks = new List<Tuple<ICodeBlock, byte[]>>();

		private readonly Dictionary<string, Function> Functions = new Dictionary<string, Function>();

		private readonly Dictionary<string, LsnType> Types = new Dictionary<string, LsnType>();

		private readonly Dictionary<string, GenericType> GenericTypes = LsnType.GetBaseGenerics().ToDictionary(g => g.Name);

		public ResourceDeserializer()
		{
			Types.Add("int", LsnType.int_);
			Types.Add("double", LsnType.double_);
			Types.Add("bool", LsnType.Bool_);
			Types.Add("string", LsnType.string_);
		}

		internal TypeId[] LoadTypeIds(BinaryDataReader reader)
		{
			var nTypes = reader.ReadUInt16();
			TypeIds = new TypeId[nTypes];
			for (int i = 0; i < nTypes; i++)
			{
				var name = reader.ReadString();
				if (Types.ContainsKey(name))
					TypeIds[i] = Types[name].Id;
				else TypeIds[i] = new TypeId(name);
			}

			return TypeIds;
		}

		private LsnType GetType(string typeName)
		{
			if (typeName.Contains('`'))
			{
				var names = typeName.Split('`');
				if (!GenericTypes.ContainsKey(names[0])) throw new ApplicationException();
				var generic = GenericTypes[names[0]];
				return generic.GetType(names.Skip(1).Select(GetType).Select(t => t.Id).ToArray());
			}
			if (Types.ContainsKey(typeName))
				return Types[typeName];
			throw new ApplicationException();
		}

		private Function GetFunction(string functionName)
		{
			if (Functions.ContainsKey(functionName))
				return Functions[functionName];
			throw new ApplicationException();
		}

		private Method GetMethod(string typeName, string methodName)
		{
			var type = GetType(typeName);
			if (!type.Methods.ContainsKey(methodName))
				throw new ApplicationException();
			return type.Methods[methodName];
		}

		private Statement ReadStatement(BinaryDataReader reader)
		{
			var code = (StatementCode)reader.ReadUInt16();
			switch (code)
			{
				case StatementCode.Return:
					return new ReturnStatement(null);
				case StatementCode.ReturnValue:
					return new ReturnStatement(ReadExpression(reader));
				case StatementCode.Jump:
					return new JumpStatement
					{
						Target = reader.ReadInt32()
					};
				case StatementCode.ConditionalJump:
					{
						var t = reader.ReadInt32();
						var cnd = ReadExpression(reader);
						return new ConditionalJumpStatement(cnd)
						{
							Target = t
						};
					}
				case StatementCode.EvaluateExpression:
					return new ExpressionStatement(ReadExpression(reader));
				case StatementCode.AssignVariable:
					{
						var index = reader.ReadUInt16();
						var val = ReadExpression(reader);
						return new AssignmentStatement(index, val);
					}
				case StatementCode.AssignField:
					{
						var index = reader.ReadUInt16();
						var target = ReadExpression(reader);
						var val = ReadExpression(reader);
						return new FieldAssignmentStatement(target, index, val);
					}
				case StatementCode.AssignValueInCollection:
					{
						var target = ReadExpression(reader);
						var index = ReadExpression(reader);
						var val = ReadExpression(reader);
						return new CollectionValueAssignmentStatement(target, index, val);
					}
				case StatementCode.SetState:
					return new SetStateStatement(reader.ReadInt32());
				case StatementCode.RegisterChoice:
					{
						var target = reader.ReadInt32();
						var cnd = ReadExpression(reader);
						var txt = ReadExpression(reader);
						return new RegisterChoiceStatement(cnd, txt)
						{
							Target = target
						};
					}
				case StatementCode.DisplayChoices:
					return new DisplayChoicesStatement();
				case StatementCode.Say:
					{
						var msg = ReadExpression(reader);
						var grph = ReadExpression(reader);
						var title = ReadExpression(reader);
						return new SayStatement(msg, grph, title);
					}
				case StatementCode.GoTo:
					throw new NotImplementedException();
				case StatementCode.GiveItem:
					{
						var amount = ReadExpression(reader);
						var id = ReadExpression(reader);
						var rcvr = ReadExpression(reader);
						return new GiveItemStatement(id, amount, rcvr);
					}
				case StatementCode.GiveGold:
					{
						var amount = ReadExpression(reader);
						var rcvr = ReadExpression(reader);
						return new GiveGoldStatement(amount, rcvr);
					}
				case StatementCode.Save:
					{
						var n = reader.ReadUInt16();
						var vars = reader.ReadUInt16s(n);
						var id = reader.ReadString();
						return new SaveStatement(vars, id);
					}
				case StatementCode.Load:
					{
						var n = reader.ReadUInt16();
						var vars = reader.ReadUInt16s(n);
						var id = reader.ReadString();
						return new LoadStatement(vars, id);
					}
				case StatementCode.Detach:
					return new DetachStatement();
				case StatementCode.AttachNewScriptObject:
					{
						var type = TypeIds[reader.ReadUInt16()];
						var nArgs = reader.ReadByte();
						var args = new IExpression[nArgs];
						for (int i = 0; i < nArgs; i++)
							args[i] = ReadExpression(reader);
						return new AttachStatement(type, args, ReadExpression(reader));
					}

				case StatementCode.SetTarget:
					{
						var index = reader.ReadUInt16();
						var target = reader.ReadInt32();
						return new SetTargetStatement(index) { Target = target };
					}
				case StatementCode.JumpToTarget:
					return new JumpToTargetStatement(reader.ReadUInt16());
				case StatementCode.Extension1:
				case StatementCode.Extension2:
				default:
					throw new ApplicationException();
			}
		}

		/// <summary>
		/// Reads the index (UInt16) from the stream and returns the associated TypeId
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		internal TypeId LoadTypeId(BinaryDataReader reader)
			=> TypeIds[reader.ReadUInt16()];

		private IExpression ReadExpression(BinaryDataReader reader)
		{
			var code = (ExpressionCode)reader.ReadByte();
			switch (code)
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
				case ExpressionCode.HostInterfaceAccess:
					return new HostInterfaceAccessExpression();
				case ExpressionCode.UniqueScriptObjectAccess:
					{
						var typeId = TypeIds[reader.ReadUInt16()];
						return new UniqueScriptObjectAccessExpression(typeId);
					}
				case ExpressionCode.BinaryExpression:
					{
						var info = reader.ReadByte();
						var argType = (BinaryOperationArgsType)(info >> 4);
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
						var typeName = TypeIds[reader.ReadUInt16()].Name;
						var methodName = reader.ReadString();
						var numArgs = reader.ReadByte();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new MethodCall(GetMethod(typeName, methodName), args);
					}
				/*case ExpressionCode.ScriptObjectMethodCall:
					{
						var methodName = reader.ReadString();
						var numArgs = reader.ReadByte();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new ScriptObjectVirtualMethodCall(args, methodName);
					}*/
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
						var type = TypeIds[reader.ReadUInt16()];
						var numArgs = reader.ReadUInt16();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new RecordConstructor(type, args);
					}
				case ExpressionCode.StructConstructor:
					{
						var type = TypeIds[reader.ReadUInt16()];
						var numArgs = reader.ReadUInt16();
						var args = new IExpression[numArgs];
						for (int i = 0; i < numArgs; i++)
							args[i] = ReadExpression(reader);
						return new StructConstructor(type, args);
					}
				case ExpressionCode.ListConstructor:
					{
						var typeId = TypeIds[reader.ReadUInt16()];
						var type = LsnListGeneric.Instance.GetType(new TypeId[] { typeId });
						return new ListConstructor((LsnListType)type);
					}
				case ExpressionCode.RangeExpression:
					{
						var start = ReadExpression(reader);
						var end = ReadExpression(reader);
						return new RangeExpression(start, end);
					}
				default:
					throw new ApplicationException();
			}
		}

		internal void ResolveCodeBlocks()
		{
			foreach (var codeBlock in CodeBlocks)
			{
				using (var stream = new MemoryStream(codeBlock.Item2, false))
				{
					using (var reader = new BinaryDataReader(stream, true))
					{
						var nStatements = reader.ReadUInt16();
						var statements = new Statement[nStatements];
						for (int i = 0; i < nStatements; i++)
							statements[i] = ReadStatement(reader);
						codeBlock.Item1.Code = statements;
					}
				}
			}
		}

		internal void ReadConstantTable(BinaryDataReader reader)
		{
			var nConstants = reader.ReadUInt16();
			ConstantTable = new ILsnValue[nConstants];
			for (int i = 0; i < nConstants; i++)
				ConstantTable[i] = ReadConstant(reader);
		}

		private static ILsnValue ReadConstant(BinaryDataReader reader)
		{
			switch ((ConstantCode)reader.ReadByte())
			{
				case ConstantCode.String:
					return new StringValue(reader.ReadString());
				case ConstantCode.Record:
					{
						var nFields = reader.ReadUInt16();
						var fields = new LsnValue[nFields];
						for (int i = 0; i < nFields; i++)
							fields[i] = ReadValue(reader);
						return new RecordValue(fields);
					}
				case ConstantCode.Range:
					{
						var start = reader.ReadInt32();
						var end = reader.ReadInt32();
						return new RangeValue(start, end);
					}
				case ConstantCode.Vector:
				default:
					throw new ApplicationException();
			}
		}

		internal void RegisterFunction(LsnFunction fn, byte[] code)
		{
			Functions.Add(fn.Name, fn);
			RegisterCodeBlock(fn, code);
		}

		internal void RegisterCodeBlock(ICodeBlock codeBlock, byte[] code)
		{
			CodeBlocks.Add(new Tuple<ICodeBlock, byte[]>(codeBlock, code));
		}

		internal void LoadFunctions(IEnumerable<Function> functions)
		{
			foreach (var fn in functions)
				Functions.Add(fn.Name, fn);
		}

		internal void LoadTypes(IEnumerable<LsnType> types)
		{
			foreach (var type in types)
				if (!Types.ContainsKey(type.Name))
					Types.Add(type.Name, type);
		}

		public static LsnValue ReadValue(BinaryDataReader reader)
		{
			switch ((ConstantCode)reader.ReadByte())
			{
				case ConstantCode.DoubleOrInt:
					return new LsnValue(reader.ReadDouble());
				case ConstantCode.String:
					return new LsnValue(new StringValue(reader.ReadString()));
				case ConstantCode.Record:
					{
						var nFields = reader.ReadUInt16();
						var fields = new LsnValue[nFields];
						for (int i = 0; i < nFields; i++)
							fields[i] = ReadValue(reader);
						return new LsnValue(new RecordValue(fields));
					}
				case ConstantCode.Struct:
					{
						var nFields = reader.ReadUInt16();
						var fields = new LsnValue[nFields];
						for (int i = 0; i < nFields; i++)
							fields[i] = ReadValue(reader);
						return new LsnValue(new StructValue(fields));
					}
				case ConstantCode.Vector:
				case ConstantCode.List:
					throw new NotImplementedException();
				default:
					throw new ApplicationException();
			}
		}

		public static LsnValue ReadValue(BinaryDataReader reader, IResourceManager resourceManager)
		{
			switch ((ConstantCode)reader.ReadByte())
			{
				case ConstantCode.DoubleOrInt:
					return new LsnValue(reader.ReadDouble());
				case ConstantCode.String:
					return new LsnValue(new StringValue(reader.ReadString()));
				case ConstantCode.Record:
					{
						var nFields = reader.ReadUInt16();
						var fields = new LsnValue[nFields];
						for (int i = 0; i < nFields; i++)
							fields[i] = ReadValue(reader);
						return new LsnValue(new RecordValue(fields));
					}
				case ConstantCode.Struct:
					{
						var nFields = reader.ReadUInt16();
						var fields = new LsnValue[nFields];
						for (int i = 0; i < nFields; i++)
							fields[i] = ReadValue(reader);
						return new LsnValue(new StructValue(fields));
					}
				case ConstantCode.Vector:
					{
						var typeName = reader.ReadString();
						var nValues = reader.ReadUInt16();
						var values = new LsnValue[nValues];
						for (int i = 0; i < nValues; i++)
							values[i] = ReadValue(reader);
						var type = (VectorType)resourceManager.GetLsnType(typeName);
						return new LsnValue(new VectorInstance(type, values));
					}
				case ConstantCode.List:
					{
						var typeName = reader.ReadString();
						var nValues = reader.ReadUInt16();
						var values = new List<LsnValue>(nValues);
						for (int i = 0; i < nValues; i++)
							values.Add(ReadValue(reader));
						var listType = (LsnListType)resourceManager.GetLsnType(typeName);
						return new LsnValue(new LsnList(listType, values));
					}
				case ConstantCode.HostInterface:
					{
						switch (Settings.HostInterfaceIdType)
						{
							case IdentifierType.Numeric:
								return new LsnValue(resourceManager.GetHostInterface(reader.ReadUInt32()));
							case IdentifierType.Text:
								return new LsnValue(resourceManager.GetHostInterface(reader.ReadString()));
							default:
								throw new ApplicationException();
						}
					}
				case ConstantCode.ScriptObject:
					{
						return new LsnValue(ReadScriptObjectReference(reader, resourceManager));
					}

				case ConstantCode.Range:
					throw new NotImplementedException();
				default:
					throw new ApplicationException();
			}
		}

		public static ScriptObject ReadScriptObjectReference(BinaryDataReader reader, IResourceManager resourceManager)
		{
			if (reader.ReadBoolean())
			{
				return resourceManager.GetUniqueScriptObject(reader.ReadString());
			}
			switch (Settings.ScriptObjectIdFormat)
			{
				case ScriptObjectIdFormat.Host_Self:
					switch (Settings.ScriptObjectIdType)
					{
						case IdentifierType.Numeric:
							switch (Settings.HostInterfaceIdType)
							{
								case IdentifierType.Numeric:
									return resourceManager.GetScriptObject(reader.ReadUInt32(), reader.ReadUInt32());
								case IdentifierType.Text:
									return resourceManager.GetScriptObject(reader.ReadString(), reader.ReadUInt32());
								default:
									throw new ApplicationException();
							}
						case IdentifierType.Text:
							switch (Settings.HostInterfaceIdType)
							{
								case IdentifierType.Numeric:
									return resourceManager.GetScriptObject(reader.ReadUInt32(), reader.ReadString());
								case IdentifierType.Text:
									return resourceManager.GetScriptObject(reader.ReadString(), reader.ReadString());
								default:
									throw new ApplicationException();
							}
					}
					break;
				case ScriptObjectIdFormat.Self:
					switch (Settings.ScriptObjectIdType)
					{
						case IdentifierType.Numeric:
							return resourceManager.GetScriptObject(reader.ReadUInt32());
						case IdentifierType.Text:
							return resourceManager.GetScriptObject(reader.ReadString());
						default:
							throw new ApplicationException();
					}
				default:
					throw new ApplicationException();
			}
			throw new ApplicationException();
		}

		public static ScriptObject ReadScriptObject(BinaryDataReader reader, IResourceManager resourceManager, IHostInterface host)
		{
			var typeName = reader.ReadString();
			var currentState = reader.ReadInt32();
			var type = (ScriptClass)resourceManager.GetLsnType(typeName);
			var fields = new LsnValue[type.NumberOfFields];
			for (int i = 0; i < type.NumberOfFields; i++)
				fields[i] = ReadValue(reader, resourceManager);

			return new ScriptObject(fields, type, currentState, host);
		}

		public static ScriptObject ReadScriptObject(BinaryDataReader reader, IResourceManager resourceManager, bool canHaveHost)
		{
			if (!canHaveHost) return ReadScriptObject(reader, resourceManager, null);
			IHostInterface host = null;
			switch (Settings.HostInterfaceIdType)
			{
				case IdentifierType.Numeric:
					var numId = reader.ReadUInt32();
					if (numId != 0)
						host = resourceManager.GetHostInterface(numId);
					break;
				case IdentifierType.Text:
					var txtId = reader.ReadString();
					if (txtId.Length > 0)
						host = resourceManager.GetHostInterface(txtId);
					break;
				default:
					throw new Exception("Unexpected Case");
			}
			return ReadScriptObject(reader, resourceManager, host);
		}
	}
}
