﻿using LsnCore.Debug;
using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	public interface IProcedureB
	{
		Instruction[] Instructions { get; }
		ushort NumberOfParameters { get; }
		ushort StackSize { get; }
		LsnObjectFile Environment { get; }
		ProcedureLineInfo LineInfo { get; }
	}

	public class LsnObjectFile
	{
		public string FileName { get; }

		internal LsnValue GetInt(ushort index) => throw new NotImplementedException();

		internal LsnValue GetDouble(ushort index) => throw new NotImplementedException();

		internal LsnValue GetObject(ushort index) => throw new NotImplementedException();

		internal LsnType GetUsedType(ushort index) => throw new NotImplementedException();

		internal LsnType GetContainedType(string name) => throw new NotImplementedException();

		/// <summary>
		/// Get a procedure called by code in this file.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal IProcedureB GetProcedure(ushort index) => throw new NotImplementedException();

		internal IProcedureB GetContainedFunction(string name) => throw new NotImplementedException();

		internal LsnObjectFile GetFile(ushort index) => throw new NotImplementedException();

		/// <summary>
		/// Gets a string that is the name of something
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal string GetString(ushort index) => throw new NotImplementedException();
	}

	struct VMRegisterFile
	{
		public int NextInstruction;

		public int Target;

		internal IProcedureB CurrentProcedure;
	}

	/*
	 *	Old calling convention:
	 *		The caller places the arguments into the stack frame.
	 *		
	 *	New calling convention:
	 *		Only 'self' is put onto the stack. The callee pops the arguments from the eval stack. This allows very simple procedures to avoid
	 *		allocating space on their stack for parameters that are used only once and in the reverse order that they are passed...
	 */
	public class VirtualMachine
	{
		static readonly LsnValue Cairo = new LsnValue(new StringValue("Elephant"));

		readonly TypeId[] OneTypeArray = new TypeId[1];

		readonly IResourceManager	ResourceManager;
		readonly ILsnGameHost		GameHost;

		VMRegisterFile RegisterFile = new VMRegisterFile();

		int Target
		{
			get => RegisterFile.Target;
			set => RegisterFile.Target = value;
		}

		int NextInstruction
		{
			get => RegisterFile.NextInstruction;
			set => RegisterFile.NextInstruction = value;
		}

		readonly LsnVMStack Stack;

		readonly Stack<LsnValue> EvalStack = new Stack<LsnValue>();

		public VirtualMachine(IResourceManager resourceManager, ILsnGameHost gameHost)
		{
			ResourceManager = resourceManager; GameHost = gameHost; Stack = new LsnVMStack(ResourceManager);
		}

		IProcedureB CurrentProcedure
		{
			get => RegisterFile.CurrentProcedure;
			set => RegisterFile.CurrentProcedure = value;
		}
		Instruction[] Code => CurrentProcedure.Instructions;
		LsnObjectFile Environment => CurrentProcedure.Environment;

		int CurrentInstruction;

		void Push(int v)       => EvalStack.Push(new LsnValue(v));
		void Push(uint v)      => EvalStack.Push(new LsnValue(v));
		void Push(bool b)      => EvalStack.Push(new LsnValue(b));
		void Push(double v)    => EvalStack.Push(new LsnValue(v));
		void Push(string v)    => EvalStack.Push(new LsnValue(new StringValue(v)));
		void Push(LsnValue v)  => EvalStack.Push(v);
		void Push(ILsnValue v) => EvalStack.Push(new LsnValue(v));

		LsnValue Pop()     => EvalStack.Pop();
		int PopI32()       => EvalStack.Pop().IntValue;
		uint PopUI32()     => EvalStack.Pop().HandleData;
		double PopF64()    => EvalStack.Pop().DoubleValue;
		bool PopBool()     => EvalStack.Pop().BoolValueSimple;
		string PopString() => (EvalStack.Pop().Value as StringValue).Value;

		LsnValue Peek() => EvalStack.Peek();
		public LsnValue Enter(IProcedureB procedure, LsnValue[] args)
		{
			Push(Cairo);
			NextInstruction = -1;
			Stack.EnterProcedure(RegisterFile, procedure);
			CurrentProcedure = procedure;
			NextInstruction = 0;
			CurrentInstruction = 0;

			while (CurrentInstruction > -1 && CurrentInstruction < Code.Length)
			{
				Eval(Code[CurrentInstruction]);
				CurrentInstruction = NextInstruction++;
			}
			var res = EvalStack.Pop();
			EvalStack.Clear();
			return res;
		}

		ushort TmpIndex;

		void Eval(Instruction instr)
		{
			var opCode = instr.OpCode;
			switch (opCode)
			{
				case OpCode.Nop:                                                  break;
				case OpCode.Dup:	 Push(Peek());                                break;
				case OpCode.Pop:	 Pop();                                       break;
				#region Arithmetic
				case OpCode.Add:     Push(LsnValue.DoubleSum(Pop(), Pop()));      break;
				case OpCode.Sub:     Push(LsnValue.DoubleDiff(Pop(), Pop()));     break;
				case OpCode.Mul_I32: Push(LsnValue.IntProduct(Pop(), Pop()));     break;
				case OpCode.Mul_F64: Push(LsnValue.DoubleProduct(Pop(), Pop()));  break;
				case OpCode.Div_I32: Push(LsnValue.IntQuotient(Pop(), Pop()));    break;
				case OpCode.Div_F64: Push(LsnValue.DoubleQuotient(Pop(), Pop())); break;
				case OpCode.Rem:     Push(LsnValue.IntMod(Pop(), Pop()));         break;
				case OpCode.Pow_F64: Push(LsnValue.DoublePow(Pop(), Pop()));      break;
				case OpCode.Neg:     Push(-Pop().DoubleValue);                    break;
				#endregion
				#region Strings
				case OpCode.Concat:      Push(PopString() + PopString());         break;
				case OpCode.IntToString: Push(PopI32().ToString());               break;
				#endregion
				#region Compare
				case OpCode.Eq_I32:			Push(PopI32() == PopI32());								break;
				case OpCode.Eq_F64:			Push(Math.Abs(PopF64() - PopF64()) < double.Epsilon);	break;
				case OpCode.Eq_Str:			Push(PopString() == PopString());						break;
				case OpCode.Neq_I32:		Push(PopI32() != PopI32());								break;
				case OpCode.Neq_F64:		Push(Math.Abs(PopF64() - PopF64()) < double.Epsilon);	break;
				case OpCode.Neq_Str:		Push(PopString() != PopString());						break;
				case OpCode.NonNull:		Push(!Pop().IsNull);									break;
				case OpCode.NonNull_NoPop:	Push(!Peek().IsNull);									break;
				case OpCode.Lt:				Push(PopF64() < PopF64());								break;
				case OpCode.Lte:			Push(PopF64() <= PopF64());								break;
				case OpCode.Gte:			Push(PopF64() >= PopF64());								break;
				case OpCode.Gt:				Push(PopF64() > PopF64());								break;
				#endregion
				case OpCode.MakeRange: Push(new RangeValue(PopI32(), PopI32())); break;
				#region Logic
				case OpCode.And:	Push(PopBool() && PopBool());	break;
				case OpCode.Or:		Push(PopBool() || PopBool());	break;
				case OpCode.Not:	Push(!PopBool());				break;
				#endregion
				//case OpCode.Conv_I32_F64:break;
				case OpCode.Jump:			NextInstruction = instr.Index;					break;
				case OpCode.Jump_True:		if (PopBool())  NextInstruction = instr.Index;	break;
				case OpCode.Jump_False:		if (!PopBool()) NextInstruction = instr.Index;	break;
				case OpCode.JumpToTarget:	NextInstruction = Target;						break;
				case OpCode.SetTarget:		Target = instr.Index;							break;
				#region Call
				case OpCode.LoadIndex:		TmpIndex = instr.Index;							break;
				case OpCode.CallFn_Short:
				case OpCode.CallFn:
					EnterFunction(Environment.GetProcedure(instr.Index)); break;
				case OpCode.CallFn_Local:
					//EnterFunction(Environment.GetProcedure(instr.Index));break;
					throw new NotImplementedException();
				case OpCode.CallNativeMethod:
					{
						var methodName = Environment.GetString(instr.Index);
						var type = Environment.GetUsedType(TmpIndex);
						var method = (BoundedMethod)type.Methods[methodName];
						if (method.ReturnType != null && method.ReturnType.Name != "void")
							Push(method.Eval(EvalStack));
						else method.Eval(EvalStack);
					} break;
				case OpCode.CallLsnMethod:
					{
						/*var type = Environment.GetUsedType(TmpIndex);
						var proc = (IProcedureB)type.Methods[Environment.GetString(instr.Index)];*/
						EnterFunction(Environment.GetProcedure(instr.Index));
					}
					break;
				case OpCode.CallScObjMethod:
					{
						/*var type = (ScriptClass)Environment.GetUsedType(TmpIndex);
						var method = type.GetMethod(Environment.GetString(instr.Index));
						var obj = Peek().Value as ScriptObject;
						var proc = (IProcedureB)obj.GetMethod(Environment.GetString(instr.Index));*/
						EnterFunction(Environment.GetProcedure(instr.Index));
					}
					break;
				case OpCode.CallScObjVirtualMethod:
					{
						throw new NotImplementedException();
					}
				case OpCode.CallHostInterfaceMethod:
					{
						var obj = Peek().Value as IHostInterface;
						var type = obj.Type.Type;
						throw new NotImplementedException();
					}
				case OpCode.Ret:
					RegisterFile = Stack.ExitProcedure();
					break;
				#endregion
				case OpCode.LoadConst_I32_short:		Push(instr.Data);								break;
				case OpCode.LoadConst_I32:				Push(Environment.GetInt(instr.Index));			break;
				case OpCode.LoadConst_F64:				Push(Environment.GetDouble(instr.Index));		break;
				case OpCode.LoadConst_F64_short:		Push((double)instr.Data);						break;
				case OpCode.LoadConst_F64_ShortRatio:	Push(instr.Index / ((double)ushort.MaxValue));	break;
				case OpCode.LoadConst_Obj:				Push(Environment.GetObject(instr.Index));		break;
				case OpCode.LoadConst_Nil:				Push(LsnValue.Nil);								break;
				case OpCode.Load_UniqueScriptClass:
					Push(ResourceManager.GetUniqueScriptObject(Environment.GetUsedType(instr.Index) as ScriptClass));
					break;
				case OpCode.LoadLocal: Push(Stack.GetVariable(instr.Index)); break;
				case OpCode.StoreLocal: Stack.SetVariable(instr.Index, Pop()); break;
				case OpCode.LoadElement: {
						var col = Pop().Value as ICollectionValue;
						Push(col.GetValue(Pop().IntValue));
					} break;
				case OpCode.StoreElement: {
						var col = Pop().Value as IWritableCollectionValue;
						var index = Pop().IntValue; var val = Pop(); col.SetValue(index, val);
					} break;
				case OpCode.ConstructList:
					Push(new LsnList((LsnListType)Environment.GetUsedType(instr.Index)));
					break;
				case OpCode.InitializeList:
				case OpCode.InitializeVector:
					throw new NotImplementedException();
				case OpCode.LoadField: Push((Pop().Value as IHasFieldsValue).GetFieldValue(instr.Index)); break;
				case OpCode.StoreField: {
						var obj = Pop().Value as IHasMutableFieldsValue;
						obj.SetFieldValue(instr.Index, Pop());
					} break;
				case OpCode.ConstructStruct: {
						var type = Environment.GetUsedType(instr.Index) as StructType;
						var fCount = type.FieldCount;
						var vals = GetArray(fCount);
						for (int i = fCount - 1; i >= 0; i++)
							vals[i] = Pop();
						Push(new StructValue(type.Id, vals));
					} break;
				case OpCode.CopyStruct: {
						var str = Pop().Value as StructValue;
						Push(str.Clone());
					} break;
				case OpCode.FreeStruct: {
						var str = Pop().Value as StructValue;
						var fCount = instr.Index; //(str.Type.Type as StructType).FieldCount;
						FreeArray(str.Values, fCount);
					} break;
				case OpCode.ConstructRecord: {
						var type = Environment.GetUsedType(instr.Index) as RecordType;
						var fCount = type.FieldCount;
						var vals = GetArray(fCount);
						for (int i = fCount - 1; i >= 0; i++)
							vals[i] = Pop();
						Push(new RecordValue(vals, type.Id));
					} break;
				case OpCode.FreeRecord:{
						var rec = Pop().Value as RecordValue;
						var fCount = instr.Index;//(rec.Type.Type as RecordType)
					} break;
				#region Script Class
				case OpCode.ConstructScriptClass:
					{
						var scriptClass = Environment.GetUsedType(instr.Index) as ScriptClass;
						var cstor = scriptClass.Constructor;
						/*Push(new ScriptObject(new LsnValue[scriptClass.Fields.Count], scriptClass, scriptClass.DefaultStateId, null));
						Push(Peek());// This way it's the return value...*/
						EnterFunction((IProcedureB)cstor);
					} break;
				case OpCode.CreateScriptClass:
					{
						var scriptClass = Environment.GetUsedType(instr.Index) as ScriptClass;
						var scrObj = new ScriptObject(new LsnValue[scriptClass.Fields.Count], scriptClass, scriptClass.DefaultStateId, null);
						break;
					}
				case OpCode.CreateAndAttachScriptClass:
					{
						var scriptClass = Environment.GetUsedType(instr.Index) as ScriptClass;
						var scrObj = new ScriptObject(new LsnValue[scriptClass.Fields.Count], scriptClass, scriptClass.DefaultStateId, Pop().Value as IHostInterface, false);
						break;
					}
				case OpCode.RegisterScriptObjectForEvents:
					{
						var scrObj = Stack.GetVariable(0).Value as ScriptObject;
						scrObj.RegisterForEvents();
						break;
					}
				case OpCode.SetState:	(Stack.GetVariable(0).Value as ScriptObject).SetState(instr.Data);	break;
				case OpCode.Detach:		(Stack.GetVariable(0).Value as ScriptObject).Detach();				break;
				case OpCode.GetHost:	Push((Stack.GetVariable(0).Value as ScriptObject).GetHost());		break;
				#endregion
				#region LSN
				case OpCode.GoTo:
				case OpCode.ComeFrom:
					throw new NotImplementedException();
				case OpCode.Say:			GameHost.Say(PopString(), Pop(), PopString());		break;
				case OpCode.RegisterChoice:	GameHost.RegisterChoice(PopString(), instr.Index);	break;
				case OpCode.CallChoices:		NextInstruction = GameHost.DisplayChoices(); GameHost.ClearChoices(); break;
				case OpCode.GiveItem:
				case OpCode.GiveGold:
					throw new NotImplementedException();
				#endregion
				case OpCode.ReadString:		Push(GameHost.GetString(PopString()));			break;
				case OpCode.ReadInt:		Push(GameHost.GetInt(PopString()));				break;
				case OpCode.ReadDouble:		Push(GameHost.GetDouble(PopString()));			break;
				case OpCode.Srand:			GameHost.RngSetSeed(PopI32());					break;
				case OpCode.Rand:			Push(GameHost.RngGetDouble());					break;
				case OpCode.RandInt:		Push(GameHost.RngGetInt(PopI32(), PopI32()));	break;
				#region Debug
				case OpCode.Error:
				case OpCode.AssertHostReturnIs_I32:
				case OpCode.AssertHostReturnIs_F64:
				case OpCode.AssertHostReturnIs_Type:
				case OpCode.AssertHostReturnIs_Option_I32:
				case OpCode.AssertHostReturnIs_Option_F64:
				case OpCode.AssertHostReturnIs_Option_Type:
					throw new NotImplementedException();
				#endregion
				case OpCode.CRN:
					throw new NotImplementedException();
				case OpCode.HCF:
					throw new NotImplementedException();
				default:
					break;
			}
		}

		void EnterFunction(IProcedureB procedure)
		{
			Stack.EnterProcedure(RegisterFile, procedure);
			CurrentProcedure = procedure;
			// Place args in stack frame...
			/*for (int i = CurrentProcedure.NumberOfParameters - 1; i >= 0; i--)
				Stack.SetVariable(i, Pop());*/
			NextInstruction = 0; // All procedures start at 0;
		}

		// Used by script class constructors...
		void EnterMethod(IProcedureB procedure)
		{
			Stack.EnterProcedure(RegisterFile, procedure);
			CurrentProcedure = procedure;
			// Place args in stack frame...
			Stack.SetVariable(0, Pop());
			for (int i = CurrentProcedure.NumberOfParameters - 1; i > 0; i--)
				Stack.SetVariable(i, Pop());
			NextInstruction = 0; // All procedures start at 0;
		}

		//private static int NearestPower(int i) => 1 << (int)Math.Ceiling(Math.Log(i, 2));

		LsnValue[] GetArray(int size) => new LsnValue[size];

		void FreeArray(LsnValue[] array, int size)
		{
			for (var i = 0; i < size; i++)
				array[i] = LsnValue.Nil;
		}
	}
}
