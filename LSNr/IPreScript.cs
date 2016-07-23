using LsnCore;

namespace LSNr
{
	public interface IPreScript : ITypeContainer
	{
		Scope CurrentScope { get; set; }
		bool Mutable { get; }
        bool Valid { get; set; }

		bool FunctionExists(string name);
		bool FunctionIsIncluded(string name);
		Function GetFunction(string name);
	}
}