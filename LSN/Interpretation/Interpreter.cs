using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Values;
using LsnCore.Types;

namespace LsnCore
{
	public abstract class Interpreter : IInterpreter
	{
		public IResourceManager ResourceManager { get; set; }

		public LsnValue ReturnValue { get; set; }

		public int JumpTarget { get; set; } = -1;
		public int NextStatement { get; set; }

		private readonly LsnStack Stack = new LsnStack();

		private readonly List<Tuple<string, int>> _Choices = new List<Tuple<string, int>>();
		protected IReadOnlyList<Tuple<string, int>> Choices => _Choices;

		protected Interpreter() { }

		public virtual void RunProcedure(IProcedure procedure)
		{
			Stack.EnterProcedure(NextStatement, JumpTarget, procedure);
			var code = procedure.Code;
			NextStatement = 0;
			var currentStatement = NextStatement++;
			var codeSize = code.Length;
			var v = InterpretValue.Base;
			while (currentStatement < codeSize && v != InterpretValue.Return)
			{
				v = code[currentStatement].Interpret(this);
				currentStatement = NextStatement++;
			}
			NextStatement = Stack.ExitProcedure(out int j);
			JumpTarget = j;
		}

		public virtual void RunProcedure(IProcedure procedure, LsnValue[] args)
		{
			Stack.EnterProcedure(NextStatement, JumpTarget, procedure, args);
			var code = procedure.Code;
			NextStatement = 0;
			var currentStatement = NextStatement++;
			var codeSize = code.Length;
			var v = InterpretValue.Base;
			while (currentStatement < codeSize && v != InterpretValue.Return)
			{
				v = code[currentStatement].Interpret(this);
				currentStatement = NextStatement++;
			}
			NextStatement = Stack.ExitProcedure(out int j);
			JumpTarget = j;
		}

		/// <summary>
		/// Get a function.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Function GetFunction(string name)
			=> Stack.GetFunction(name);

		/// <summary>
		/// Get the variable at the provided index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public LsnValue GetVariable(int index)
			=> Stack.GetVariable(index);

		/// <summary>
		/// Set the value of the variable at the provided index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void SetVariable(int index, LsnValue value)
		{
			Stack.SetVariable(index, value);
		}

		private static int NearestPower(int i)
			=> 1 << (int)Math.Ceiling(Math.Log(i, 2));

		/// <summary>
		/// Get the unique script object.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ScriptObject GetUniqueScriptObject(string name) => ResourceManager.GetUniqueScriptObject(Stack.CurrentEnvironment.ScriptClasses[name]);

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
			for (int i = 0; i < indexes.Length; i++)
				values[i] = Stack.GetVariable(indexes[i]);
			ResourceManager.SaveValues(values, saveId);
		}

		public void LoadVariables(ushort[] indexes, string saveId)
		{
			var values = ResourceManager.LoadValues(saveId);
			for (int i = 0; i < indexes.Length; i++)
				Stack.SetVariable(indexes[i], values[i]);
		}

		protected Random Rng = new Random();

		public virtual void RngSetSeed(int seed)
		{
			Rng = new Random(seed);
		}

		public virtual int RngGetInt(int min, int max)
			=> Rng.Next(min, max);

		public virtual double RngGetDouble()
			=> Rng.NextDouble();
	}
}