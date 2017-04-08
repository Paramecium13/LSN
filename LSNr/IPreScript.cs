using LsnCore;

namespace LSNr
{
	// All variable,field,etc. type symbols in this enum must come before Function.
	public enum SymbolType { Undefined, Variable, GlobalVariable, Field, Property, ScriptObjectMethod, HostInterfaceMethod, Function}

	public interface IPreScript : ITypeContainer
	{
		IScope CurrentScope { get; set; }
		bool Mutable { get; }
        bool Valid { get; set; }

		SymbolType CheckSymbol(string name);

		bool FunctionExists(string name);
		bool FunctionIsIncluded(string name);
		Function GetFunction(string name);
	}
}