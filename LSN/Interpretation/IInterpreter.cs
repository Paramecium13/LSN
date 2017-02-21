using System.Collections.Generic;

namespace LsnCore
{
	public interface IInterpreter
	{
		LsnValue ReturnValue { get; set; }

		/// <summary>
		/// Should CompoundExpressions pass variable names to this (rather than their values), may save time.
		/// </summary>
		bool PassVariablesByName { get; }
		
		/// <summary>
		/// Enters a new scope for interpreting a function. Previously defined variables are inaccessable.
		/// </summary>
		void EnterFunctionScope(LsnEnvironment env, int scopeSize);

		/// <summary>
		/// Exits the scope of the current function.
		/// </summary>
		void ExitFunctionScope();

		IActor GetActor(LsnValue id);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		LsnValue GetValue(int index);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		void SetValue(int index, LsnValue value);

		/// <summary>
		/// Get a function.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Function GetFunction(string name);


		void GiveArmorTo(LsnValue id, int amount, LsnValue target);


		void GiveGoldTo(int amount, LsnValue target);


		void GiveItemTo(LsnValue id, int amount, LsnValue target);


		void GiveWeaponTo(LsnValue id, int amount, LsnValue target);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="graphic"> The id of the graphic to display, null if no graphic should be displayed.</param>
		/// <param name="title">The title to display, null if no title should be displayed.</param>
		void Say(string message, LsnValue graphic, string title);

		/// <summary>
		/// Has the player make a choice and returns the index of that choice.
		/// </summary>
		/// <param name="choices"></param>
		/// <returns></returns>
		int Choice(List<string> choices); // TODO: Replace with Dictionary<string,int>.


		LsnValue GetGlobalVariable(string globalVarName/*, string fileName*/);

		void SetGlobalVariable(LsnValue value, string globalVarName/*, string fileName*/);

		void WatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange);

		void UnwatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange);

	}
}