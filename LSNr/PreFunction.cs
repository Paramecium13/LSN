using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;

namespace LSNr
{
	public class PreFunction : IPreScript
	{
		private readonly PreResource Resource;

		internal PreFunction(PreResource resource)
		{
			Resource = resource;
		}

		private IScope _CurrentScope = new VariableTable(new List<Variable>());

		public IScope CurrentScope { get { return _CurrentScope; } set { _CurrentScope = value; } } 

		public bool Mutable => Resource.Mutable;

		public bool Valid {get { return Resource.Valid; } set { Resource.Valid = value; } }

		public bool FunctionExists(string name) => Resource.FunctionExists(name);

		public bool FunctionIsIncluded(string name) => Resource.FunctionIsIncluded(name);

		public bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);

		public Function GetFunction(string name) => Resource.GetFunction(name);

		public GenericType GetGenericType(string name) => Resource.GetGenericType(name);

		public LsnType GetType(string name) => Resource.GetType(name);

		public bool TypeExists(string name) => Resource.TypeExists(name);


		public SymbolType CheckSymbol(string name)
		{
			if (FunctionExists(name))
				return SymbolType.Function;
			if (_CurrentScope.VariableExists(name))
				return SymbolType.Variable;

			return SymbolType.Undefined;
		}

		public TypeId GetTypeId(string name) => Resource.GetTypeId(name);
	}
}
