namespace LSN_Core
{
	public interface IScope<T> where T : IScope<T>
	{
		void AddVariable(string name, ILSN_Value val);
		ILSN_Value GetValue(string name);
		T Pop();
		T Push();
		void ReAssignVariable(string name, ILSN_Value val);
	}
}