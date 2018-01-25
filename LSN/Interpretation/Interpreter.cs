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

		private readonly static ConcurrentDictionary<int, ConcurrentStack<LsnValue[]>> StackFrameStore = new ConcurrentDictionary<int, ConcurrentStack<LsnValue[]>>();

		private LsnValue[] CurrentStackFrame;

		private readonly Stack<LsnValue[]> StackFrames = new Stack<LsnValue[]>();

		// Where the current environment is pushed when a new function scope is entered.
		private readonly Stack<LsnEnvironment> EnvStack = new Stack<LsnEnvironment>();

		// The current environment.
		private LsnEnvironment CurrentEnvironment;

		public int NextStatement { get; set; }
		private static Stack<int> NextStatementStack = new Stack<int>();

		private readonly List<Tuple<string, int>> _Choices = new List<Tuple<string, int>>();
		protected IReadOnlyList<Tuple<string, int>> Choices => _Choices;

		protected Interpreter()
		{
			EnvStack.Push(null);
			NextStatementStack.Push(-4);
			StackFrames.Push(new LsnValue[0]);
		}

		public void Run(Statements.Statement[] code, string resourceFilePath, int stackSize, LsnValue[] parameters)
		{
			EnterFunctionScope(resourceFilePath, stackSize);
			for (int i = 0; i < parameters.Length; i++)
				CurrentStackFrame[i] = parameters[i];

			NextStatement = 0;
			var currentStatement = NextStatement++;
			var codeSize = code.Length;
			var v = InterpretValue.Base;
			while (currentStatement < codeSize && v != InterpretValue.Return)
			{
				v = code[currentStatement].Interpret(this);
				currentStatement = NextStatement++;
			}
		}

		/*/// <summary>
		/// Enters a new scope for interpreting a function. Previously defined variables are inaccessable.
		/// </summary>
		/// <param name="env">todo: describe env parameter on EnterFunctionScope</param>
		/// <param name="scopeSize">todo: describe scopeSize parameter on EnterFunctionScope</param>
		public virtual void EnterFunctionScope(LsnEnvironment env, int scopeSize)
		{
			if(CurrentEnvironment != null)
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
		}*/

		public virtual void EnterFunctionScope(string resourceFilePath, int scopeSize)
		{
			NextStatementStack.Push(NextStatement);
			if(CurrentEnvironment != null)
				EnvStack.Push(CurrentEnvironment);
			try
			{
				CurrentEnvironment = ResourceManager.GetResource(resourceFilePath).GetEnvironment(ResourceManager);
			}
			catch (Exception e)
			{
				throw new ApplicationException("ResourceManager exception", e);
			}
			if(CurrentStackFrame != null)
				StackFrames.Push(CurrentStackFrame);
			CurrentStackFrame = RequestStack(scopeSize);
		}

		private static LsnValue[] RequestStack(int scopeSize)
		{
			LsnValue[] x = null;
			var i = NearestPower(scopeSize);
			if (StackFrameStore.ContainsKey(i))
			{
				if (!StackFrameStore[i].TryPop(out x))
					return new LsnValue[i];
				return x;
			}
			StackFrameStore.TryAdd(i, new ConcurrentStack<LsnValue[]>());
			return new LsnValue[i];
		}

		/// <summary>
		/// Exits the scope of the current function.
		/// </summary>
		public virtual void ExitFunctionScope()
		{
			RecycleStack(CurrentStackFrame);
			CurrentStackFrame = StackFrames.Pop();
			CurrentEnvironment = EnvStack.Pop();
			NextStatement = NextStatementStack.Pop();
		}

		/// <summary>
		/// Get a function.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Function GetFunction(string name)
			=> CurrentEnvironment.Functions[name];

		/// <summary>
		/// Get the variable at the provided index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public LsnValue GetVariable(int index)
			=> CurrentStackFrame[index];

		/// <summary>
		/// Set the value of the variable at the provided index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void SetVariable(int index, LsnValue value)
		{
			CurrentStackFrame[index] = value;
		}

		private static void RecycleStack(LsnValue[] stack)
		{
			Task.Run(() =>
			{
				for (int i = 0; i < stack.Length; i++)
					stack[i] = LsnValue.Nil;
				if (StackFrameStore.ContainsKey(stack.Length))
					StackFrameStore[stack.Length].Push(stack);
				else
				{
					StackFrameStore.TryAdd(stack.Length, new ConcurrentStack<LsnValue[]>());
					StackFrameStore[stack.Length].Push(stack);
				}
			});
		}

		private static int NearestPower(int i)
			=> 1 << (int)Math.Ceiling(Math.Log(i, 2));

		/// <summary>
		/// Get the unique script object.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ScriptObject GetUniqueScriptObject(string name)
		{
			return ResourceManager.GetUniqueScriptObject(name);
		}

		//protected abstract GlobalVariableValue GetGlobalVariableValue(string globalVarName/*, string fileName*/);

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

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="id"></param>
		/// <param name="amount"></param>
		/// <param name="target"></param>
		public abstract void GiveItemTo(LsnValue id, int amount, LsnValue target);

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="target"></param>
		public abstract void GiveGoldTo(int amount, LsnValue target);

		/// <summary>
		/// Display a message to the player with an optional title and graphic.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="graphic"> The id of the graphic to display, null if no graphic should be displayed.</param>
		/// <param name="title">The title to display, null if no title should be displayed.</param>
		public abstract void Say(string message, LsnValue graphic, string title);

		/// <summary>
		/// Register a choice for the player and the index of the instruction to jump to if the player selects that choice.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="target"></param>
		public void RegisterChoice(string text, int target)
		{
			_Choices.Add(new Tuple<string, int>(text, target));
		}

		/// <summary>
		/// Clear the registered choices.
		/// </summary>
		public void ClearChoices()
		{
			_Choices.Clear();
		}

		/// <summary>
		/// Display the registered choices to the player and return the index of the instruction to jump to.
		/// </summary>
		/// <returns></returns>
		public abstract int DisplayChoices();

		public void SaveVariables(ushort[] indexes, string saveId)
		{
			var values = new LsnValue[indexes.Length];
			for(int i = 0; i < indexes.Length; i++)
				values[i] = CurrentStackFrame[indexes[i]];
			ResourceManager.SaveValues(values, saveId);
		}

		public void LoadVariables(ushort[] indexes, string saveId)
		{
			var values = ResourceManager.LoadValues(saveId);
			for (int i = 0; i < indexes.Length; i++)
				CurrentStackFrame[indexes[i]] = values[i];
		}
	}
}