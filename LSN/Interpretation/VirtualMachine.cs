using LsnCore.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	interface IProcedureB
	{
		Instruction[] Code { get; }
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

	class FileEnvironment
	{
		public string FileName { get; }

		public LsnValue GetInt(ushort index) => throw new NotImplementedException();

		public LsnValue GetDouble(ushort index) => throw new NotImplementedException();

		public LsnValue GetObject(ushort index) => throw new NotImplementedException();

		public LsnType GetType(ushort index) => throw new NotImplementedException();

		public Function GetFunction(ushort index) => throw new NotImplementedException();

		public FileEnvironment GetFile(ushort index) => throw new NotImplementedException();

		public string GetString(ushort index) => throw new NotImplementedException();
	}

	abstract class VirtualMachine
	{
		public IResourceManager ResourceManager { get; set; }

		public LsnValue ReturnValue { get; set; }

		int NextStatement { get; set; }

		readonly LsnVMStack Stack;

		readonly Stack<LsnValue> EvalStack = new Stack<LsnValue>();

		public VirtualMachine(IResourceManager resourceManager) { ResourceManager = resourceManager; Stack = new LsnVMStackB(ResourceManager); }

		Instruction[] Code;
		int CurrentStatement;

		public void Enter(IProcedureB procedure, LsnValue[] args)
		{
			Stack.EnterProcedure(NextStatement, procedure);
			Code = procedure.Code;
			NextStatement = 0;
			CurrentStatement = 0;
			while (CurrentStatement < Code.Length)
			{

				CurrentStatement = NextStatement++;
			}
		}

		void Eval(Instruction instruction)
		{
			var opCode = instruction.OpCode;
			switch (opCode)
			{
				case OpCode.Nop:	break;
				case OpCode.Add:		EvalStack.Push(LsnValue.DoubleSum(EvalStack.Pop(), EvalStack.Pop()));		break;
				case OpCode.Sub:		EvalStack.Push(LsnValue.DoubleDiff(EvalStack.Pop(), EvalStack.Pop()));		break;
				case OpCode.Mul_I32:	EvalStack.Push(LsnValue.IntProduct(EvalStack.Pop(), EvalStack.Pop()));		break;
				case OpCode.Mul_F64:	EvalStack.Push(LsnValue.DoubleProduct(EvalStack.Pop(), EvalStack.Pop()));	break;
				case OpCode.Div_I32:
					break;
				case OpCode.Div_F64:
					break;
				case OpCode.Rem:
					break;
				case OpCode.Pow_F64:
					break;
				case OpCode.Neg:
					break;
				case OpCode.Concat:
					break;
				case OpCode.IntToString:
					break;
				case OpCode.Eq_I32:
					break;
				case OpCode.Eq_F64:
					break;
				case OpCode.Eq_Str:
					break;
				case OpCode.Neq_I32:
					break;
				case OpCode.Neq_F64:
					break;
				case OpCode.Neq_Str:
					break;
				case OpCode.NonNull:
					break;
				case OpCode.Lt:
					break;
				case OpCode.Lte:
					break;
				case OpCode.Gte:
					break;
				case OpCode.Gt:
					break;
				case OpCode.MakeRange:
					break;
				case OpCode.And:
					break;
				case OpCode.Or:
					break;
				case OpCode.Not:
					break;
				case OpCode.Conv_I32_F64:
					break;
				case OpCode.Jump:
					break;
				case OpCode.Jump_True:
					break;
				case OpCode.Jump_Target:
					break;
				case OpCode.Set_Target:
					break;
				case OpCode.LoadIndex:
					break;
				case OpCode.CallFn:
					break;
				case OpCode.CallFn_Short:
					break;
				case OpCode.CallFn_Local:
					break;
				case OpCode.CallMethod:
					break;
				case OpCode.CallHostInterfaceMethod:
					break;
				case OpCode.Ret:
					break;
				case OpCode.LoadConst_I32_short:
					break;
				case OpCode.LoadConst_I32:
					break;
				case OpCode.LoadConst_F64_short:
					break;
				case OpCode.LoadConst_F64:
					break;
				case OpCode.LoadConst_Obj:
					break;
				case OpCode.LoadConst_Nil:
					break;
				case OpCode.Load_UniqueScriptClass:
					break;
				case OpCode.LoadLocal:
					break;
				case OpCode.StoreLocal:
					break;
				case OpCode.LoadElement:
					break;
				case OpCode.StoreElement:
					break;
				case OpCode.ConstructList:
					break;
				case OpCode.InitializeList:
					break;
				case OpCode.InitializeVector:
					break;
				case OpCode.LoadField:
					break;
				case OpCode.StoreField:
					break;
				case OpCode.ConstructStruct:
					break;
				case OpCode.CopyStruct:
					break;
				case OpCode.ConstructRecord:
					break;
				case OpCode.SetState:
					break;
				case OpCode.ConstructScriptClass:
					break;
				case OpCode.ConstructAndAttachScriptClass:
					break;
				case OpCode.Detach:
					break;
				case OpCode.GetHost:
					break;
				case OpCode.GoTo:
					break;
				case OpCode.Say:
					break;
				case OpCode.RegisterChoice:
					break;
				case OpCode.CallChoice:
					break;
				case OpCode.GiveItem:
					break;
				case OpCode.GiveGold:
					break;
				default:
					break;
			}
		}
	}
}
