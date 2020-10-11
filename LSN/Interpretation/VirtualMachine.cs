using LsnCore.Debug;
using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	public interface IProcedureB
	{
		ushort NumberOfParameters { get; }
		ushort StackSize { get; }
		LsnObjectFile Environment { get; }
		ProcedureLineInfo LineInfo { get; }
		ProcedureInfo Info { get; }
	}

	/// <summary>
	/// Contains the 'Registers' of the virtual machine: $PC, $Target, & $Procedure.
	/// </summary>
	internal struct VMRegisterFile
	{
		public int NextInstruction;

		public int Target;

		internal ProcedureInfo CurrentProcedure;
	}

	/*
	 *	Calling convention:
	 *		For non-virtual calls, the callee pops its arguments from the eval stack as it sees fit...
	 */
	// ReSharper disable once UnusedMember.Global
	public class VirtualMachine
	{
		static readonly LsnValue Cairo = new LsnValue(new StringValue("Elephant"));

		readonly TypeId[] OneTypeArray = new TypeId[1];

		readonly IResourceManager	ResourceManager;
		readonly ILsnGameHost		GameHost;

		VMRegisterFile RegisterFile;

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

		readonly LsnVmEvalStack EvalStack = new LsnVmEvalStack();

		public VirtualMachine(IResourceManager resourceManager, ILsnGameHost gameHost)
		{
			ResourceManager = resourceManager; GameHost = gameHost; Stack = new LsnVMStack(ResourceManager);
		}

		ProcedureInfo CurrentProcedure
		{
			get => RegisterFile.CurrentProcedure;
			set => RegisterFile.CurrentProcedure = value;
		}
		Instruction[] Code => CurrentProcedure.File.Code;
		LsnObjectFile Environment => CurrentProcedure.File;

		int CurrentInstruction;

		void Push(int v)       => EvalStack.Push(new LsnValue(v));
		void Push(uint v)      => EvalStack.Push(new LsnValue(v));
		void Push(bool b)      => EvalStack.Push(new LsnValue(b));
		void Push(double v)    => EvalStack.Push(new LsnValue(v));
		void Push(string v)    => EvalStack.Push(new LsnValue(new StringValue(v)));
		void Push(LsnValue v)  => EvalStack.Push(v);
		void Push(ILsnValue v) => EvalStack.Push(new LsnValue(v));

		LsnValue Pop()         => EvalStack.Pop();
		int      PopI32()      => EvalStack.Pop().IntValue;
		uint     PopUI32()     => EvalStack.Pop().HandleData;
		double   PopF64()      => EvalStack.Pop().DoubleValue;
		bool     PopBool()     => EvalStack.Pop().BoolValueSimple;
		string   PopString()   => ((StringValue) EvalStack.Pop().Value).Value;

		LsnValue Peek() => EvalStack.Peek();

		//ToDo: Make VMRegisterFile a stack variable/parameter...
		public LsnValue Enter(ProcedureInfo procedure, LsnValue[] args)
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

		short TmpIndex;

		void Eval(Instruction instr)
		{
			var opCode = instr.OpCode;
			switch (opCode)
			{
				case OpCode.Nop:                                                  break;
				case OpCode.Dup:	 Push(Peek());                                break;
				case OpCode.Swap:
					{
						var a = Pop();
						var b = Pop();
						Push(a); Push(b);
					}break;
				case OpCode.SwaDup:
					{
						var a = Pop();
						var b = Pop();
						Push(a); Push(b); Push(a);
					}break;
				case OpCode.Pop:	 Pop();                                       break;
				#region Arithmetic
				case OpCode.Add:     Push(LsnValue.DoubleSum(Pop(), Pop()));      break;
				case OpCode.Sub:     Push(LsnValue.DoubleDiff(Pop(), Pop()));     break;
				//case OpCode.Mul_I32: Push(LsnValue.IntProduct(Pop(), Pop()));     break;
				case OpCode.Mul:     Push(LsnValue.DoubleProduct(Pop(), Pop()));  break;
				case OpCode.Div_I32: Push(LsnValue.IntQuotient(Pop(), Pop()));    break;
				case OpCode.Div_F64: Push(LsnValue.DoubleQuotient(Pop(), Pop())); break;
				case OpCode.Rem_I32: Push(LsnValue.IntMod(Pop(), Pop()));         break;
				case OpCode.Rem_F64: Push(LsnValue.DoubleMod(Pop(), Pop()));      break;
				case OpCode.Pow:	 Push(LsnValue.DoublePow(Pop(), Pop()));      break;
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
				//case OpCode.Eq_F64_epsilon:
				//case OpCode.Neq_F64_epsilon:
				//	throw new NotImplementedException();
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
				#region Control Flow
				case OpCode.Jump:			NextInstruction = instr.Index;					break;
				case OpCode.Jump_True:		if (PopBool())  NextInstruction = instr.Index;	break;
				case OpCode.Jump_False:		if (!PopBool()) NextInstruction = instr.Index;	break;
				case OpCode.Switch:			throw new NotImplementedException();
				case OpCode.JumpToTarget:	NextInstruction = Target;						break;
				case OpCode.SetTarget:		Target = instr.Index;							break;
				#endregion
				#region Call
				case OpCode.LoadTempIndex:		TmpIndex = instr.Data;						break;
				case OpCode.CallFn_Short:
					EnterFunction(Environment.GetFile((ushort) (instr.Index & 0xFF))
						.GetProcedure(Environment.GetIdentifierString((ushort) (instr.Index >> 8))));
					break;
				case OpCode.CallFn:
					EnterFunction(Environment.GetFile(Unsafe.As<short, ushort>(ref TmpIndex))
						.GetProcedure(Environment.GetIdentifierString(instr.Index)));
					break;
				case OpCode.CallFn_Local:
					EnterFunction(Environment.GetProcedure(instr.Index));
					break;
				case OpCode.CallNativeMethod:
					{
						var methodName = Environment.GetIdentifierString(instr.Index);
						var type = Environment.GetUsedType(TmpIndex);
						var method = (BoundedMethod)type.Methods[methodName];
						if (method.ReturnType != null && method.ReturnType.Name != "void")
							Push(method.Eval(EvalStack));
						else method.Eval(EvalStack);
					} break;
				case OpCode.CallScObjMethod:
				//case OpCode.CallLsnMethod:
					//EnterFunction(Environment.GetProcedure(instr.Index));
					//break;
				case OpCode.CallScObjVirtualMethod:
				{
					throw new NotImplementedException();
				}
				case OpCode.CallHostInterfaceMethodVoid:
				{
					var sigStub = Environment.GetSignatureStub(instr.Index);
					var args = new LsnValue[sigStub.NumberOfParameters]; // ToDo: Array Caching...
					for (var j= sigStub.NumberOfParameters - 1; j >= 0; j--)
					{
						args[j] = Pop();
					}
					var obj = (IHostInterface) Pop().Value;
					obj.CallMethod(sigStub.Name, args);
					break;
				}
				case OpCode.CallHostInterfaceMethod:
				{
					var sigStub = Environment.GetSignatureStub(instr.Index);
					var args = new LsnValue[sigStub.NumberOfParameters]; // ToDo: Array Caching...
					for (var j = sigStub.NumberOfParameters - 1; j >= 0; j--)
					{
						args[j] = Pop();
					}
					var obj = (IHostInterface) Pop().Value;

					Push(obj.CallMethod(sigStub.Name, args));
					break;
				}
				case OpCode.Ret:
					RegisterFile = Stack.ExitProcedure();
					break;
				#endregion
				#region Constants
				case OpCode.LoadConst_I32_short:		Push(instr.Data);								break;
				case OpCode.LoadConst_I32:				Push((TmpIndex << 16) | instr.Index);			break;
				case OpCode.LoadConst_F64:				Push(Environment.GetDouble(instr.Index));		break;
				//case OpCode.LoadConst_F64_short:		Push((double)instr.Data);						break;
				case OpCode.LoadConst_F64_ShortRatio:	Push(instr.Index / ((double)ushort.MaxValue));	break;
				case OpCode.LoadConst_String:			Push(Environment.GetString(instr.Index));		break;
				case OpCode.LoadConst_Nil:				Push(LsnValue.Nil);								break;
				case OpCode.LoadConst_True:				Push(true);										break;
				case OpCode.LoadConst_False:			Push(false);									break;
				case OpCode.Load_UniqueScriptClass:
					Push(ResourceManager.GetUniqueScriptObject(Environment.GetUsedType(instr.Data) as ScriptClass));
					break;
				#endregion
				#region Variables, fields, and elements
				case OpCode.LoadLocal:		Push(Stack.GetVariable(instr.Index));		break;
				case OpCode.StoreLocal:		Stack.SetVariable(instr.Index, Pop());		break;
				case OpCode.LoadElement: {
						var index = PopI32();
						var col = Pop().Value as ICollectionValue;
						Push(col.GetValue(index));
					} break;
				case OpCode.StoreElement: {
						var val = Pop();
						var index = PopI32();
						var col = Pop().Value as IMutableCollectionValue; col.SetValue(index, val);
					} break;
				case OpCode.LoadField: Push((Pop().Value as IHasFieldsValue).GetFieldValue(instr.Index)); break;
				case OpCode.StoreField: {
						var obj = Pop().Value as IHasMutableFieldsValue;
						obj.SetFieldValue(instr.Index, Pop());
					} break;
				#endregion
				#region INC and DEC
				case OpCode.PreInc_Var:
					{
						var val = new LsnValue(Stack.GetVariable(instr.Index).IntValue + 1);
						Stack.SetVariable(instr.Index, val);
						Push(val);
					}
					break;
				case OpCode.PreInc_Elem:
					{
						var index = PopI32();
						var col = Pop().Value as IMutableCollectionValue;
						var val = new LsnValue(col.GetValue(index).IntValue + 1);
						Push(val);
						col.SetValue(index, val);
					}
					break;
				case OpCode.PreInc_Fld:
					{
						var obj = Pop().Value as IHasMutableFieldsValue;
						var val = new LsnValue(obj.GetFieldValue(instr.Index).IntValue + 1);
						obj.SetFieldValue(instr.Index, val);
						Push(val);
					}
					break;
				case OpCode.PostInc_Var:
					{
						var val = Stack.GetVariable(instr.Index);
						Push(val);
						Stack.SetVariable(instr.Index, new LsnValue(val.IntValue + 1));
					}
					break;
				case OpCode.PostInc_Elem:
					{
						var index = PopI32();
						var col = Pop().Value as IMutableCollectionValue;
						var val = col.GetValue(index);
						Push(val);
						val = new LsnValue(val.IntValue + 1);
						col.SetValue(index, val);
					}
					break;
				case OpCode.PostInc_Fld:
					{
						var obj = Pop().Value as IHasMutableFieldsValue;
						var val = obj.GetFieldValue(instr.Index);
						Push(val);
						val = new LsnValue(val.IntValue + 1);
						obj.SetFieldValue(instr.Index, val);
					}
					break;
				case OpCode.PreDec_Var:
					{
						var val = new LsnValue(Stack.GetVariable(instr.Index).IntValue - 1);
						Stack.SetVariable(instr.Index, val);
						Push(val);
					}
					break;
				case OpCode.PreDec_Elem:
					{
						var index = PopI32();
						var col = Pop().Value as IMutableCollectionValue;
						var val = new LsnValue(col.GetValue(index).IntValue - 1);
						Push(val);
						col.SetValue(index, val);
					}
					break;
				case OpCode.PreDec_Fld:
					{
						var obj = Pop().Value as IHasMutableFieldsValue;
						var val = new LsnValue(obj.GetFieldValue(instr.Index).IntValue - 1);
						obj.SetFieldValue(instr.Index, val);
						Push(val);
					}
					break;
				case OpCode.PostDec_Var:
					{
						var val = Stack.GetVariable(instr.Index);
						Push(val);
						Stack.SetVariable(instr.Index, new LsnValue(val.IntValue - 1));
					}
					break;
				case OpCode.PostDec_Elem:
					{
						var index = PopI32();
						var col = Pop().Value as IMutableCollectionValue;
						var val = col.GetValue(index);
						Push(val);
						val = new LsnValue(val.IntValue - 1);
						col.SetValue(index, val);
					}
					break;
				case OpCode.PostDec_Fld:
					{
						var obj = Pop().Value as IHasMutableFieldsValue;
						var val = obj.GetFieldValue(instr.Index);
						Push(val);
						val = new LsnValue(val.IntValue - 1);
						obj.SetFieldValue(instr.Index, val);
					}
					break;
				#endregion
				#region Vectors and Lists
				case OpCode.ConstructList:
					Push(new LsnList((LsnListType)Environment.GetUsedType(instr.Data)));
					break;
				case OpCode.InitializeList:
				case OpCode.InitializeArray:
					throw new NotImplementedException();
				#endregion
				case OpCode.ConstructStruct: {
						var type = Environment.GetUsedType(instr.Data) as StructType;
						var fCount = type.FieldCount;
						var vals = GetArray(fCount);
						for (var i = fCount - 1; i >= 0; i++)
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
						var type = Environment.GetUsedType(instr.Data) as RecordType;
						var fCount = type.FieldCount;
						var vals = GetArray(fCount);
						for (var i = fCount - 1; i >= 0; i++)
							vals[i] = Pop();
						Push(new RecordValue(vals, type.Id));
					} break;
				case OpCode.FreeRecord:{
						var rec = Pop().Value as RecordValue;
						var fCount = instr.Index;//(rec.Type.Type as RecordType)
						FreeArray(rec.Fields, fCount);
					} break;
				#region Script Class
				case OpCode.ConstructScriptClass:
					{
						var scriptClass = (ScriptClass) Environment.GetUsedType(instr.Data);
						var scrObj = new ScriptObject(new LsnValue[scriptClass.Fields.Count], scriptClass, scriptClass.DefaultStateId, null);
						var cstor = scriptClass.Constructor;
						EnterFunction(((IProcedureB)cstor).Info);
						// Done after entering the procedure so that it is in the constructor's stack
						Stack.SetVariable(0, new LsnValue(scrObj));
						break;
					}
				case OpCode.ConstructAndAttachScriptClass:
					{
						var scriptClass = Environment.GetUsedType(instr.Data) as ScriptClass;
						var scrObj = new ScriptObject(new LsnValue[scriptClass.Fields.Count], scriptClass, scriptClass.DefaultStateId, (IHostInterface) Pop().Value, false);
						Push(scrObj);
						var cstor = scriptClass.Constructor;
						EnterFunction(((IProcedureB)cstor).Info);
						// Done after entering the procedure so that it is in the constructor's stack
						Stack.SetVariable(0, new LsnValue(scrObj));
						break;
					}
				case OpCode.RegisterScriptObjectForEvents:
					{
						var scrObj = (ScriptObject) Stack.GetVariable(0).Value;
						scrObj.RegisterForEvents();
						break;
					}
				case OpCode.SetState:	((ScriptObject) Stack.GetVariable(0).Value).SetState(instr.Data);	break;
				case OpCode.Detach:		((ScriptObject) Stack.GetVariable(0).Value).Detach();				break;
				case OpCode.GetHost:	Push(((ScriptObject) Stack.GetVariable(0).Value).GetHost());		break;
				#endregion
				#region LSN
				case OpCode.GoTo:
				case OpCode.ComeFrom:
					throw new NotImplementedException();
				case OpCode.Say:				GameHost.Say(PopString(), Pop(), PopString());							break;

				case OpCode.RegisterChoice_Pop:
				case OpCode.RegisterChoice:		GameHost.RegisterChoice(PopString(), instr.Index);						break;
				case OpCode.CallChoices:		NextInstruction = GameHost.DisplayChoices(); GameHost.ClearChoices();	break;
				case OpCode.CallChoices_Push:	Push(GameHost.DisplayChoices()); GameHost.ClearChoices();				break;
				case OpCode.GiveItem:
				case OpCode.GiveGold:
					throw new NotImplementedException();
				#endregion
				#region Input
				case OpCode.ReadString:		Push(GameHost.GetString(PopString()));			break;
				case OpCode.ReadInt:		Push(GameHost.GetInt(PopString()));				break;
				case OpCode.ReadDouble:		Push(GameHost.GetDouble(PopString()));			break;
				#endregion
				#region Rand
				case OpCode.Srand:			GameHost.RngSetSeed(PopI32());					break;
				case OpCode.Srand_sysTime:	throw new NotImplementedException();
				case OpCode.Rand:			Push(GameHost.RngGetDouble());					break;
				case OpCode.RandInt:
				{
					var min = PopI32();
					var max = PopI32();
					Push(GameHost.RngGetInt(min, max));
					break;
				}
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
				#region Math
				case OpCode.Min:	 Push(Math.Min(PopF64(), PopF64()));		break;
				case OpCode.Max:	 Push(Math.Max(PopF64(), PopF64()));		break;
				case OpCode.Floor:	 Push(Math.Floor(PopF64()));				break;
				case OpCode.Ceil:	 Push(Math.Ceiling(PopF64()));				break;
				case OpCode.Round:	 Push(Math.Round(PopF64()));				break;
				case OpCode.Abs:	 Push(Math.Abs(PopF64()));					break;
				case OpCode.Sqrt:	 Push(Math.Sqrt(PopF64()));					break;
				case OpCode.Sin:	 Push(Math.Sin(PopF64()));					break;
				case OpCode.Cos:	 Push(Math.Cos(PopF64()));					break;
				case OpCode.Tan:	 Push(Math.Tan(PopF64()));					break;
				case OpCode.ASin:	 Push(Math.Asin(PopF64()));					break;
				case OpCode.ACos:	 Push(Math.Acos(PopF64()));					break;
				case OpCode.ATan:	 Push(Math.Atan(PopF64()));					break;
				#endregion
				case OpCode.CRN:
				case OpCode.CFRN:
				case OpCode.HCF:
					throw new NotImplementedException();
				default:
					break;
			}
		}

		void EnterFunction(ProcedureInfo procedure)
		{
			Stack.EnterProcedure(RegisterFile, procedure);
			CurrentProcedure = procedure;
			// Place args in stack frame...
			for (int i = CurrentProcedure.NumberOfParameters - 1; i >= 0; i--)
				Stack.SetVariable(i, Pop());
			NextInstruction = procedure.CodeOffset;
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
