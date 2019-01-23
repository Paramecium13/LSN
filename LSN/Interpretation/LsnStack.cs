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
		private int Count;		// == Offset + Current Frame Size
		private int Offset;
		private readonly Stack<FrameInfo> Frames = new Stack<FrameInfo>();
		internal LsnEnvironment CurrentEnvironment => Frames.Peek().Environment;

		public LsnValue GetVariable(int index) => Values[Offset + index];
		public void SetVariable(int index, LsnValue value)
		{
			Values[Offset + index] = value;
		}

		public Function GetFunction(string name) => CurrentEnvironment.Functions[name];

		public void EnterProcedure(int nextStatement, int jumpTarget, IProcedure procedure, LsnValue[] args)
		{
			Frames.Push(new FrameInfo(nextStatement, jumpTarget, procedure, ResourceManager.GetResource(procedure.ResourceFilePath).GetEnvironment(ResourceManager)));
			Offset = Count;
			Count += procedure.StackSize;
			if (Count > Values.Length) Grow();
			var x = args.Length + Offset;
			for (int i = Offset; i < x; i++)
				Values[i] = args[i];
		}

		public void EnterProcedure(int nextStatement, int jumpTarget, IProcedure procedure)
		{
			Frames.Push(new FrameInfo(nextStatement, jumpTarget, procedure, ResourceManager.GetResource(procedure.ResourceFilePath).GetEnvironment(ResourceManager)));
			Offset = Count;
			Count += procedure.StackSize;
			if (Count > Values.Length) Grow();
		}

		public int ExitProcedure(out int target)
		{
			var frame = Frames.Pop();
			Array.Clear(Values, Offset, Count - Offset);
			Count = Offset;
			Offset -= frame.Procedure.StackSize;
			target = frame.Target;
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
			internal readonly int Target;
			internal readonly IProcedure Procedure;
			internal readonly LsnEnvironment Environment;

			internal FrameInfo(int nxt, int target, IProcedure proc, LsnEnvironment env)
			{
				NextStatement = nxt; Target = target; Procedure = proc; Environment = env;
			}
		}
	}
}
