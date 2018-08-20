using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;

namespace LSNr
{
	public sealed class PreScriptClassFunction : IPreScript
	{
		internal readonly BasePreScriptClass Parent;

		internal PreScriptClassFunction(BasePreScriptClass parent)
		{
			Parent = parent;
		}

		public IScope CurrentScope { get; set; } = new VariableTable(new List<Variable>());

		public bool Valid { get { return Parent.Valid; } set { Parent.Valid = value; } }

		public bool Mutable								=> Parent.Mutable;
		public string Path								=> Parent.Path;
		public bool FunctionExists(string name)			=> Parent.FunctionExists(name);
		public bool GenericTypeExists(string name)		=> Parent.GenericTypeExists(name);
		public Function GetFunction(string name)		=> Parent.GetFunction(name);
		public GenericType GetGenericType(string name)	=> Parent.GetGenericType(name);
		public LsnType GetType(string name)				=> Parent.GetType(name);
		public TypeId GetTypeId(string name)			=> Parent.GetTypeId(name);
		public bool TypeExists(string name)				=> Parent.TypeExists(name);
		public bool TypeIsIncluded(TypeId type)			=> Parent.TypeIsIncluded(type);

		public SymbolType CheckSymbol(string name)
		{
			if (CurrentScope.VariableExists(name))
				return SymbolType.Variable;
			return Parent.CheckSymbol(name);
		}

		public void GenericTypeUsed(TypeId typeId)
		{
			Parent.GenericTypeUsed(typeId);
		}
	}
}
