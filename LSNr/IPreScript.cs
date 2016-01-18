using LSN_Core;

namespace LSNr
{
	public interface IPreScript
	{
		Scope CurrentScope { get; set; }
		bool Mutable { get; set; }
        bool Valid { get; set; }

		bool FunctionExists(string name);
		Function GetFunction(string name);
		LSN_Script GetScript();
	}
}