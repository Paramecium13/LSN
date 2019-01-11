using LsnCore;
using LsnCore.Types;
using LSNr.Statements;
using System.Collections.Generic;

namespace LSNr
{
	// All variable,field,etc. type symbols in this enum must come before Function.
	public enum SymbolType { Undefined, UniqueScriptObject, Variable, GlobalVariable, Field, Property, ScriptClassMethod, HostInterfaceMethod, Function, Type}

	public interface IPreScript : ITypeContainer
	{
		IScope CurrentScope { get; set; }
		bool Mutable { get; }
		bool Valid { get; set; }
		string Path { get; }

		SymbolType CheckSymbol(string name);
		Function GetFunction(string name);

		IReadOnlyList<IStatementRule> StatementRules { get; }
	}
}