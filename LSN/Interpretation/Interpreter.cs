using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Values;

namespace LsnCore
{
	public abstract class Interpreter : IInterpreter
	{

		public IResourceManager ResourceManager { get; set; }

		public LsnValue ReturnValue { get; set; }


		//protected List<ILsnValue> LsnObjects = new List<ILsnValue>();


		private readonly Dictionary<int, ConcurrentStack<LsnValue[]>> StackFrameStore = new Dictionary<int, ConcurrentStack<LsnValue[]>>();


		private LsnValue[] CurrentStackFrame;


		private readonly Stack<LsnValue[]> StackFrames = new Stack<LsnValue[]>();




		// Where the current environment is pushed when a new function scope is entered.
		private readonly Stack<LsnEnvironment> EnvStack = new Stack<LsnEnvironment>();

		// The current environment.
		private LsnEnvironment CurrentEnvironment;


		public int NextStatement { get; set; }


		private readonly List<Tuple<string, int>> _Choices = new List<Tuple<string, int>>();
		protected IReadOnlyList<Tuple<string, int>> Choices => _Choices; 

		public void Run(Statements.Statement[] code, LsnEnvironment environment, int stackSize, LsnValue[] parameters)
		{
			EnterFunctionScope(environment, stackSize);
			for (int i = 0; i < parameters.Length; i++)
				CurrentStackFrame[i] = parameters[i];

			NextStatement = 0;
			int currentStatement = NextStatement++;
			int codeSize = code.Length;
			var v = InterpretValue.Base;
			while (currentStatement < codeSize && v != InterpretValue.Return)
			{
				v = code[currentStatement].Interpret(this);
				currentStatement = NextStatement++;
			}
		}

		public void Run(Statements.Statement[] code, string resourceFilePath, int stackSize, LsnValue[] parameters)
		{
			EnterFunctionScope(resourceFilePath, stackSize);
			for (int i = 0; i < parameters.Length; i++)
				CurrentStackFrame[i] = parameters[i];

			NextStatement = 0;
			int currentStatement = NextStatement++;
			int codeSize = code.Length;
			var v = InterpretValue.Base;
			while (currentStatement < codeSize && v != InterpretValue.Return)
			{
				v = code[currentStatement].Interpret(this);
				currentStatement = NextStatement++;
			}
		}

		public void Run(Statements.Statement[] code, LsnValue[] parameters)
		{
			for (int i = 0; i < parameters.Length; i++)
				CurrentStackFrame[i] = parameters[i];

			NextStatement = 0;
			int currentStatement = NextStatement++;
			int codeSize = code.Length;
			var v = InterpretValue.Base;
			while (currentStatement < codeSize && v != InterpretValue.Return)
			{
				v = code[currentStatement].Interpret(this);
				currentStatement = NextStatement++;
			}
		}

		/// <summary>
		/// Run the script.
		/// </summary>
		/// <param name="script"></param>
		public virtual void Run(LsnScript script)
		{
			for (int i = 0; i < script.Components.Length; i++)
			{
				var c = script.Components[i];
				c.Interpret(this);
			}
		}

		/// <summary>
		/// Enters a new scope for interpreting a function. Previously defined variables are inaccessable.
		/// </summary>
		/// <param name="env">todo: describe env parameter on EnterFunctionScope</param>
		/// <param name="scopeSize">todo: describe scopeSize parameter on EnterFunctionScope</param>
		public virtual void EnterFunctionScope(LsnEnvironment env, int scopeSize)
		{
			EnvStack.Push(CurrentEnvironment);
			CurrentEnvironment = env;

			StackFrames.Push(CurrentStackFrame);
			int i = NearestPower(scopeSize);
			if (StackFrameStore.ContainsKey(i))
			{
				if (!StackFrameStore[i].TryPop(out CurrentStackFrame))
					CurrentStackFrame = new LsnValue[i];
			}
			else
			{
				StackFrameStore.Add(i, new ConcurrentStack<LsnValue[]>());
				CurrentStackFrame = new LsnValue[i];
			}
		}

		public virtual void EnterFunctionScope(string resourceFilePath, int scopeSize)
		{
			EnvStack.Push(CurrentEnvironment);
			CurrentEnvironment = ResourceManager.GetResource(resourceFilePath).GetEnvironment();

			StackFrames.Push(CurrentStackFrame);
			int i = NearestPower(scopeSize);
			if (StackFrameStore.ContainsKey(i))
			{
				if (!StackFrameStore[i].TryPop(out CurrentStackFrame))
					CurrentStackFrame = new LsnValue[i];
			}
			else
			{
				StackFrameStore.Add(i, new ConcurrentStack<LsnValue[]>());
				CurrentStackFrame = new LsnValue[i];
			}
		}

		private void RequestStack(int scopeSize)
		{
			int i = NearestPower(scopeSize);
			if (StackFrameStore.ContainsKey(i))
			{
				if (!StackFrameStore[i].TryPop(out CurrentStackFrame))
					CurrentStackFrame = new LsnValue[i];
			}
			else
			{
				StackFrameStore.Add(i, new ConcurrentStack<LsnValue[]>());
				CurrentStackFrame = new LsnValue[i];
			}
		}

		/// <summary>
		/// Exits the scope of the current function.
		/// </summary>
		public virtual void ExitFunctionScope()
		{
			RecycleStack(CurrentStackFrame);
			CurrentStackFrame = StackFrames.Pop();
			CurrentEnvironment = EnvStack.Pop();
		}

		/// <summary>
		/// Get a function.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Function GetFunction(string name)
			=> CurrentEnvironment.Functions[name];


		public LsnValue GetVariable(int index)
			=> CurrentStackFrame[index];


		public void SetVariable(int index, LsnValue value)
		{
			CurrentStackFrame[index] = value;
		}


		private void RecycleStack(LsnValue[] stack)
		{
			Task.Run(() =>
			{
				for (int i = 0; i < stack.Length; i++)
					stack[i] = LsnValue.Nil;
				if (StackFrameStore.ContainsKey(stack.Length))
					StackFrameStore[stack.Length].Push(stack);
				else
				{
					StackFrameStore.Add(stack.Length, new ConcurrentStack<LsnValue[]>());
					StackFrameStore[stack.Length].Push(stack);
				}
			});
		}


		private static int NearestPower(int i)
			=> 1 << (int)Math.Ceiling(Math.Log(i, 2));



		public ScriptObject GetUniqueScriptObject(/*string path,*/ string name)
		{
			return ResourceManager.GetUniqueScriptObject(name);
		}


		protected abstract GlobalVariableValue GetGlobalVariableValue(string globalVarName/*, string fileName*/);


		//public LsnValue GetGlobalVariable(string globalVarName/*, string fileName*/)
		//	=> GetGlobalVariableValue(globalVarName/*, filename*/).Value;


		//public void SetGlobalVariable(LsnValue value, string globalVarName/*, string fileName*/)
		//{
		//	GetGlobalVariableValue(globalVarName/*, fileName*/).Value = value;
		//}


		//public virtual void WatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange)
		//{
		//	(GetGlobalVariableValue(globalVarName/*, fileName*/) as GlobalVariableValueWatched).OnValueChanged += onChange;
		//}

		//public virtual void UnwatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange)
		//{
		//	(GetGlobalVariableValue(globalVarName/*, fileName*/) as GlobalVariableValueWatched).OnValueChanged -= onChange;
		//}

		public void RegisterChoice(string text, int target)
		{
			_Choices.Add(new Tuple<string, int>(text, target));
		}

		public void ClearChoices()
		{
			_Choices.Clear();
		}

		/*
		#region unsafe
		// Test for using unmanaged stuff...
		public unsafe virtual void AddVariable(int id, IntValue val, object dummyParam)
		{
			int v = val.Value;
			int* ptr = &v;
			IntPtr x = Marshal.AllocHGlobal(4);
			int* ptr2 = (int*)x.ToPointer();
			*ptr2 = v;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">The LSN type that will be stored.</param>
		/// <param name="number">The number of elements in the array.</param>
		/// <param name="id"  >An identifier, for other parts of the script to access this array.</param>
		public unsafe virtual void AllocArray(LSN_Type type, int number, int id)
		{
			var size = Marshal.SizeOf(type.CreateDefaultValue());
			var ptr = Marshal.AllocHGlobal(size * number + sizeof(int));
			Arrays[id] = ptr;
			*((int*)ptr.ToPointer()) = size;
		}

		public unsafe virtual void AssignArray(ILSN_Value[] collection, int id)
		{
			var count = collection.Length;
			var tmpPtr = (int*)(Arrays[id].ToPointer());
			var size = *(tmpPtr);
			var ptr = new IntPtr(tmpPtr + sizeof(int));
			for (int i = 0; i < count; i++)
			{
				Marshal.StructureToPtr(collection[i], ptr, false);
				ptr += size;
			}
		}

		public unsafe virtual ILSN_Value GetArrayValue(int id, int index)
		{
			var tmpPtr = (int*)(Arrays[id].ToPointer());
			var size = *(tmpPtr);
			var ptr = new IntPtr(tmpPtr + sizeof(int));
			ILSN_Value x = null;
			Marshal.PtrToStructure(ptr + index * size, x);
			return x;
		}

		public unsafe virtual void SetArrayValue(int id, int index, ILSN_Value value)
		{
			var tmpPtr = (int*)(Arrays[id].ToPointer());
			var size = *(tmpPtr);
			var ptr = new IntPtr(tmpPtr + sizeof(int));
			Marshal.StructureToPtr(value, ptr + index * size, true);
		}

		public unsafe virtual void DeallocArray(int id)
		{
			Marshal.FreeHGlobal(Arrays[id]);
		}

		#endregion*/

		public abstract void GiveItemTo(LsnValue id, int amount, LsnValue target);
		public abstract void GiveArmorTo(LsnValue id, int amount, LsnValue target);
		public abstract void GiveWeaponTo(LsnValue id, int amount, LsnValue target);
		public abstract void GiveGoldTo(int amount, LsnValue target);

		//public abstract IActor GetActor(LsnValue id);

		public abstract void Say(string message, LsnValue graphic, string title);
		public abstract int Choice(List<string> choices);
		public abstract int DisplayChoices();

	}
}