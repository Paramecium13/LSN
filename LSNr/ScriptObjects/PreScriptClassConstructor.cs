using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;

namespace LSNr.ScriptObjects
{
	class PreScriptClassConstructor : IPreScript
	{
		private readonly PreScriptClass ScriptClass;

		public IScope CurrentScope { get; set; } = new VariableTable(new List<Variable>());

		public bool Mutable => ScriptClass.Mutable;
		public bool Valid { get => ScriptClass.Valid; set => ScriptClass.Valid = value; }
		public string Path => ScriptClass.Path;
		public bool FunctionExists(string name) => ScriptClass.FunctionExists(name);
		public bool FunctionIsIncluded(string name) => ScriptClass.FunctionIsIncluded(name);
		public bool GenericTypeExists(string name) => ScriptClass.GenericTypeExists(name);
		public Function GetFunction(string name) => ScriptClass.GetFunction(name);
		public GenericType GetGenericType(string name) => ScriptClass.GetGenericType(name);
		public LsnType GetType(string name) => ScriptClass.GetType(name);
		public TypeId GetTypeId(string name) => ScriptClass.GetTypeId(name);
		public bool TypeExists(string name) => ScriptClass.TypeExists(name);
		public bool TypeIsIncluded(TypeId type) => ScriptClass.TypeIsIncluded(type);

		public SymbolType CheckSymbol(string name)
		{
			if (CurrentScope.VariableExists(name))
				return SymbolType.Variable;
			return ScriptClass.CheckSymbol(name);
		}
	}
}
