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
		/// Creates a new variable with the provided name and value.
		/// </summary>
		/// <param name="name">The name of the variable to create.</param>
		/// <param name="val">The initial value to assign it.</param>
		void AddVariable(string name, LsnValue val);


		//void AddVariable(int id, IntValue val, object dummyParam);

		
		//void AllocArray(LSN_Type type, int number, int id);


		//void AssignArray(ILSN_Value[] collection, int id);


		//void DeallocArray(int id);

		/// <summary>
		/// Enters a new scope for interpreting a function. Previously defined variables are inaccessable.
		/// </summary>
		void EnterFunctionScope(LsnEnvironment env, int scopeSize);

		/// <summary>
		/// Enters a new scope, that still has access to variables defined in the previuos scope.
		/// </summary>
		void EnterScope();

		/// <summary>
		/// Exits the scope of the current function.
		/// </summary>
		void ExitFunctionScope();

		/// <summary>
		/// Exits the current scope.
		/// </summary>
		void ExitScope();


		IActor GetActor(LsnValue id);


		//ILSN_Value GetArrayValue(int id, int index);

		/// <summary>
		/// Gets the value of the specified variable.
		/// </summary>
		/// <param name="name">The name of the variable whose value is requested.</param>
		/// <returns>The value of the variable.</returns>
		//LsnValue GetValue(string name);

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


		//void SetArrayValue(int id, int index, ILSN_Value value);

		
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
		int Choice(List<string> choices);


		LsnValue GetGlobalVariable(string globalVarName/*, string fileName*/);

		void SetGlobalVariable(LsnValue value, string globalVarName/*, string fileName*/);

		void WatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange);

		void UnwatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange);

	}
}