using LsnCore.Values;
using System.Collections.Generic;

namespace LsnCore
{
	public interface IInterpreter
	{
		/// <summary>
		/// ...
		/// </summary>
		LsnValue ReturnValue { get; set; }

		/// <summary>
		/// The next statement to execute.
		/// </summary>
		int NextStatement { get; set; }

		/// <summary>
		/// Execute the provided code...
		/// </summary>
		/// <param name="code">The code to execute</param>
		/// <param name="resourceFilePath">The path of the resource file that contains this code</param>
		/// <param name="stackSize">The size of the stack</param>
		/// <param name="parameters">The parameters that are passed to the code</param>
		void Run(Statements.Statement[] code, string resourceFilePath, int stackSize, LsnValue[] parameters);

		/// <summary>
		/// Enters a new scope for interpreting a function. Previously defined variables are inaccessable.
		/// </summary>
		void EnterFunctionScope(string resourceFilePath, int scopeSize);

		/// <summary>
		/// Exits the scope of the current function.
		/// </summary>
		void ExitFunctionScope();

		/// <summary>
		/// Get the variable at the provided index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		LsnValue GetVariable(int index);

		/// <summary>
		/// Set the value of the variable at the provided index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		void SetVariable(int index, LsnValue value);

		/// <summary>
		/// Get the function with the provided name from the loaded file(s).
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Function GetFunction(string name);

		/// <summary>
		/// Get the unique script object.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		ScriptObject GetUniqueScriptObject(string name);

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="target"></param>
		void GiveGoldTo(int amount, LsnValue target);

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="id"></param>
		/// <param name="amount"></param>
		/// <param name="target"></param>
		void GiveItemTo(LsnValue id, int amount, LsnValue target);

		/// <summary>
		/// Display a message to the player with an optional title and graphic.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="graphic"> The id of the graphic to display, null if no graphic should be displayed.</param>
		/// <param name="title">The title to display, null if no title should be displayed.</param>
		void Say(string message, LsnValue graphic, string title);

		//LsnValue GetGlobalVariable(string globalVarName/*, string fileName*/);

		//void SetGlobalVariable(LsnValue value, string globalVarName/*, string fileName*/);

		//void WatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange);

		//void UnwatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange);*/

		/// <summary>
		/// Register a choice for the player and the index of the instruction to jump to if the player selects that choice.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="target"></param>
		void RegisterChoice(string text, int target);

		/// <summary>
		/// Display the registered choices to the player and return the index of the instruction to jump to.
		/// </summary>
		/// <returns></returns>
		int DisplayChoices();

		/// <summary>
		/// Clear the registered choices.
		/// </summary>
		void ClearChoices();
	}
}