using LsnCore.Expressions;
using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public class LsnStack
	{
		protected readonly IResourceManager ResourceManager;

		private LsnValue[] Values = new LsnValue[8];
		private int Count => Offset + Frames.Peek().StackSize;
		private int Offset;
		private readonly Stack<FrameInfo> Frames = new Stack<FrameInfo>();

		public LsnStack(IResourceManager resourceManager)
		{
			ResourceManager = resourceManager;
			Frames.Push(new FrameInfo(-1, 0));
		}

		public LsnValue GetVariable(int index) => Values[Offset + index];

		public void SetVariable(int index, LsnValue value) => Values[Offset + index] = value;

		public void EnterProcedure(int nextStatement, IProcedure procedure, LsnValue[] args)
		{
			Offset += Frames.Peek().StackSize;
			Frames.Push(new FrameInfo(nextStatement, procedure));
			if (Count > Values.Length) Grow();
			for (int i = 0; i < args.Length; i++)
				Values[i + Offset] = args[i];
		}

		public void EnterProcedure(int nextStatement, IProcedure procedure)
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
			Array.Clear(old,0,old.Length);
		}

		private struct FrameInfo
		{
			internal readonly int NextStatement;
			internal readonly int StackSize;

			internal FrameInfo(int nxt, IProcedure proc)
			{
				NextStatement = nxt; StackSize = proc.StackSize;
			}

			internal FrameInfo(int nxt, int sz)
			{
				NextStatement = nxt; StackSize = sz;
			}
		}
	}
}
