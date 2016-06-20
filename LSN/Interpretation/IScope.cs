namespace LsnCore
{
	public interface IScope<T> where T : IScope<T>
	{
		void AddVariable(string name, ILsnValue val);
		ILsnValue GetValue(string name);
		T Pop();
		T Push();
		void ReAssignVariable(string name, ILsnValue val);
	}
}