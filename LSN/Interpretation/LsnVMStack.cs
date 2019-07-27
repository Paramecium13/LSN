using LsnCore.Debug;
using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;

namespace LsnCore.Interpretation
{
	class LsnVMStack
	{
		protected readonly IResourceManager ResourceManager;

		LsnValue[] Values = new LsnValue[8];
		int Count => Offset + Frames.Peek().StackSize;
		int Offset;
		readonly Stack<FrameInfo> Frames = new Stack<FrameInfo>();

		public LsnVMStack(IResourceManager resourceManager)
		{
			ResourceManager = resourceManager;
			Frames.Push(new FrameInfo(new VMRegisterFile { NextInstruction = -1 }, 0));
		}

		public LsnValue GetVariable(int index) => Values[Offset + index];

		public void SetVariable(int index, LsnValue value) => Values[Offset + index] = value;

		public void EnterProcedure(VMRegisterFile registerFile, IProcedureB procedure, LsnValue[] args)
		{
			Offset += Frames.Peek().StackSize;
			Frames.Push(new FrameInfo(registerFile, procedure));
			if (Count > Values.Length) Grow();
			for (int i = 0; i < args.Length; i++)
				Values[i + Offset] = args[i];
		}

		public void EnterProcedure(VMRegisterFile registerFile, IProcedureB procedure)
		{
			Offset += Frames.Peek().StackSize;
			Frames.Push(new FrameInfo(registerFile, procedure));
			if (Count > Values.Length) Grow();
		}

		public VMRegisterFile ExitProcedure()
		{
			var frame = Frames.Pop();
			Array.Clear(Values, Offset, Count - Offset);
			Offset -= Frames.Peek().StackSize;
			return frame.RegisterFile;
		}

		void Grow()
		{
			var newStack = new LsnValue[Values.Length << 1];
			Array.Copy(Values, newStack, Values.Length);
			var old = Values;
			Values = newStack;
			Array.Clear(old, 0, old.Length);
		}

		struct FrameInfo
		{
			internal readonly VMRegisterFile RegisterFile;
			internal readonly int StackSize;

			internal FrameInfo(VMRegisterFile registerFile, IProcedureB proc)
			{
				RegisterFile = registerFile; StackSize = proc.StackSize;
			}

			internal FrameInfo(VMRegisterFile registerFile, int size)
			{
				RegisterFile = registerFile; StackSize = size;
			}
		}
	}
}