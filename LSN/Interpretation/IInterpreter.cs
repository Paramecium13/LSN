namespace LSN_Core
{
	public interface IInterpreter
	{
		ILSN_Value ReturnValue { get; set; }

		/// <summary>
		/// Should CompoundExpressions pass variable names to this (rather than their values), may save time.
		/// </summary>
		bool PassVariablesByName { get; }
		
		/// <summary>
		/// Creates a new variable with the provided name and value.
		/// </summary>
		/// <param name="name">The name of the variable to create.</param>
		/// <param name="val">The initial value to assign it.</param>
		void AddVariable(string name, ILSN_Value val);


		//void AddVariable(int id, IntValue val, object dummyParam);


		//void AllocArray(LSN_Type type, int number, int id);


		//void AssignArray(ILSN_Value[] collection, int id);


		//void DeallocArray(int id);

		/// <summary>
		/// Enters a new scope for interpreting a function. Previously defined variables are inaccessable.
		/// </summary>
		void EnterFunctionScope(LSN_Environment env);

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


		IActor GetActor(LSN_Value id);


		//ILSN_Value GetArrayValue(int id, int index);

		/// <summary>
		/// Gets the value of the specified variable.
		/// </summary>
		/// <param name="name">The name of the variable whose value is requested.</param>
		/// <returns>The value of the variable.</returns>
		ILSN_Value GetValue(string name);

		/// <summary>
		/// Get a function.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Function GetFunction(string name);

		void GivArmorTo(ILSN_Value id, int amount, ILSN_Value target);


		void GiveGoldTo(int amount, ILSN_Value target);


		void GiveItemTo(ILSN_Value id, int amount, ILSN_Value target);


		void GiveWeaponTo(ILSN_Value id, int amount, ILSN_Value target);

		/// <summary>
		/// Assigns a new value to a variable.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="val">The new value to assign it.</param>
		void ReAssignVariable(string name, ILSN_Value val);


		//void SetArrayValue(int id, int index, ILSN_Value value);

		ILSN_Value Eval(string expression);
		
	}
}