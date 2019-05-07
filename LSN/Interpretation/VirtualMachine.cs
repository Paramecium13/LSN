using LsnCore.Debug;
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
		FileEnvironment Environment { get; }
		ProcedureLineInfo LineInfo { get; }
	}

	class LsnVMStack
	{
		protected readonly IResourceManager ResourceManager;

		private LsnValue[] Values = new LsnValue[8];
		private int Count => Offset + Frames.Peek().StackSize;
		private int Offset;
		private readonly Stack<FrameInfo> Frames = new Stack<FrameInfo>();

		public LsnVMStack(IResourceManager resourceManager)
		{
			ResourceManager = resourceManager;
			Frames.Push(new FrameInfo(-1, 0));
		}

		public LsnValue GetVariable(int index) => Values[Offset + index];

		public void SetVariable(int index, LsnValue value) => Values[Offset + index] = value;

		public void EnterProcedure(int nextStatement, IProcedureB procedure, LsnValue[] args)
		{
			Offset += Frames.Peek().StackSize;
			Frames.Push(new FrameInfo(nextStatement, procedure));
			if (Count > Values.Length) Grow();
			for (int i = 0; i < args.Length; i++)
				Values[i + Offset] = args[i];
		}

		public void EnterProcedure(int nextStatement, IProcedureB procedure)
		{
			Offset += Frames.Peek().StackSize;
			Frames.Push(new FrameInfo(nextStatement, procedure));
			if (Count > Values.Length) Grow();
		}

		public int ExitProcedure()
		{
			var frame = Frames.Pop();
			Array.Clear(Values, Offset, Count - Offset);
			Offset -= Frames.Peek().StackSize;
			return frame.NextStatement;
		}

		private void Grow()
		{
			var newStack = new LsnValue[Values.Length << 1];
			Array.Copy(Values, newStack, Values.Length);
			var old = Values;
			Values = newStack;
			Array.Clear(old, 0, old.Length);
		}

		private struct FrameInfo
		{
			internal readonly int NextStatement;
			internal readonly int StackSize;

			internal FrameInfo(int nxt, IProcedureB proc)
			{
				NextStatement = nxt; StackSize = proc.StackSize;
			}

			internal FrameInfo(int nxt, int sz)
			{
				NextStatement = nxt; StackSize = sz;
			}
		}
	}

	public class FileEnvironment
	{
		public string FileName { get; }

		internal LsnValue GetInt(ushort index) => throw new NotImplementedException();

		internal LsnValue GetDouble(ushort index) => throw new NotImplementedException();

		internal LsnValue GetObject(ushort index) => throw new NotImplementedException();

		internal LsnType GetType(ushort index) => throw new NotImplementedException();

		internal IProcedureB GetFunction(ushort index) => throw new NotImplementedException();

		internal FileEnvironment GetFile(ushort index) => throw new NotImplementedException();

		internal string GetString(ushort index) => throw new NotImplementedException();
	}

	/*
	 *	Calling convention:
	 *		The caller places the arguments into the stack frame.
	 */

	public class VirtualMachine
	{
		static readonly LsnValue Cairo = new LsnValue(new StringValue("Elephant"));

		readonly TypeId[] OneTypeArray = new TypeId[1];

		public IResourceManager ResourceManager { get; set; }

		int Target;

		int NextInstruction { get; set; }

		readonly LsnVMStack Stack;

		readonly Stack<LsnValue> EvalStack = new Stack<LsnValue>();

		public VirtualMachine(IResourceManager resourceManager) { ResourceManager = resourceManager; Stack = new LsnVMStack(ResourceManager); }

		readonly Stack<IProcedureB> ProcStack = new Stack<IProcedureB>();

		IProcedureB CurrentProcedure;
		Instruction[] Code => CurrentProcedure.Instructions;
		FileEnvironment Environment => CurrentProcedure.Environment;

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
			CurrentProcedure = procedure;
			Stack.EnterProcedure(-1, procedure);
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
				case OpCode.Jump_True:		if (PopBool()) NextInstruction = instr.Index;	break;
				case OpCode.Jump_Target:	NextInstruction = Target;						break;
				case OpCode.Set_Target:		Target = instr.Index;							break;
				case OpCode.LoadIndex:		TmpIndex = instr.Index;							break;
				case OpCode.CallFn:
					EnterFunction(Environment.GetFile(TmpIndex).GetFunction(instr.Index));
					break;
				case OpCode.CallFn_Short:
					EnterFunction(Environment.GetFile((ushort)(instr.Index & 0xFF)).GetFunction((ushort)(instr.Index >> 8)));
					break;
				case OpCode.CallFn_Local:
					EnterFunction(Environment.GetFunction(instr.Index));
					break;
				case OpCode.CallNativeMethod:
					{
						var method = (BoundedMethod)Peek().Value.Type.Type.Methods[Environment.GetString(instr.Index)];
						var argc = method.Parameters.Count;
						var args = GetArray(argc);
						args[0] = Pop();
						for (var i = argc - 1; i > 0; i--)
							args[i] = Pop();
						if (method.ReturnType != null && method.ReturnType.Name != "void")
							Push(method.Eval(args));
						else method.Eval(args);
					} break;
				case OpCode.CallLsnMethod:
					{
						var type = Peek().Value.Type.Type;
						var proc = (IProcedureB)type.Methods[Environment.GetString(instr.Index)];
						EnterFunction(proc);
					}
					break;
				case OpCode.CallScObjMethod:
					{
						var obj = Peek().Value as ScriptObject;
						var proc = (IProcedureB)obj.GetMethod(Environment.GetString(instr.Index));
						EnterMethod(proc);
					}
					break;
				case OpCode.CallHostInterfaceMethod:
					{
						var obj = Peek().Value as IHostInterface;
						var type = obj.Type.Type;
						throw new NotImplementedException();
					}
				case OpCode.Ret:
					NextInstruction = Stack.ExitProcedure();
					CurrentProcedure = ProcStack.Pop();
					break;
				case OpCode.LoadConst_I32_short:		Push(instr.Data);								break;
				case OpCode.LoadConst_I32:				Push(Environment.GetInt(instr.Index));			break;
				case OpCode.LoadConst_F64:				Push(Environment.GetDouble(instr.Index));		break;
				case OpCode.LoadConst_F64_short:		Push((double)instr.Data);						break;
				case OpCode.LoadConst_F64_ShortRatio:	Push(instr.Index / ((double)ushort.MaxValue));	break;
				case OpCode.LoadConst_Obj:				Push(Environment.GetObject(instr.Index));		break;
				case OpCode.LoadConst_Nil:				Push(LsnValue.Nil);								break;
				case OpCode.Load_UniqueScriptClass:
					Push(ResourceManager.GetUniqueScriptObject(Environment.GetType(instr.Index) as ScriptClass));
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
					Push(new LsnList((LsnListType)Environment.GetType(instr.Index)));
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
						var type = Environment.GetType(instr.Index) as StructType;
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
						var type = Environment.GetType(instr.Index) as RecordType;
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
						var scriptClass = Environment.GetType(instr.Index) as ScriptClass;
						var cstor = scriptClass.Constructor;
						var argc = cstor.Parameters.Length;
						Push(new ScriptObject(new LsnValue[scriptClass.Fields.Count], scriptClass, scriptClass.DefaultStateId, null));
						Push(Peek());// This way it's the return value...
						EnterMethod((IProcedureB)cstor);
					} break;
				case OpCode.ConstructAndAttachScriptClass:
					{
						var scriptClass = Environment.GetType(instr.Index) as ScriptClass;
						var cstor = scriptClass.Constructor;
						var argc = cstor.Parameters.Length;
						var host = Pop().Value as IHostInterface;
						Push(new ScriptObject(new LsnValue[scriptClass.Fields.Count], scriptClass, scriptClass.DefaultStateId, host));
						Push(Peek());// This way it's the return value...
						EnterMethod((IProcedureB)cstor);
					}
					break;
				case OpCode.SetState:	(Stack.GetVariable(0).Value as ScriptObject).SetState(instr.Data);	break;
				case OpCode.Detach:		(Stack.GetVariable(0).Value as ScriptObject).Detach();				break;
				case OpCode.GetHost:	Push((Stack.GetVariable(0).Value as ScriptObject).GetHost());		break;
				#endregion
				#region LSN
				case OpCode.GoTo:
				case OpCode.ComeFrom:
				case OpCode.Say:
				case OpCode.RegisterChoice:
				case OpCode.CallChoice:
				case OpCode.GiveItem:
				case OpCode.GiveGold:
					throw new NotImplementedException();
				#endregion
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
			ProcStack.Push(CurrentProcedure);
			Stack.EnterProcedure(NextInstruction, procedure);
			CurrentProcedure = procedure;
			// Place args in stack frame...
			for (int i = CurrentProcedure.NumberOfParameters - 1; i >= 0; i--)
				Stack.SetVariable(i, Pop());
			NextInstruction = 0; // All procedures start at 0;
		}

		void EnterMethod(IProcedureB procedure)
		{
			ProcStack.Push(CurrentProcedure);
			Stack.EnterProcedure(NextInstruction, procedure);
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
