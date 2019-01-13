using LsnCore;
using LsnCore.Types;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.ScriptObjects
{
	public sealed class ScriptClassBuilder : IPreScriptClass
	{
		readonly IPreResource resource;
		readonly TypeId Id;

		public string Path => resource.Path;
		public bool GenericTypeExists(string name) => resource.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => resource.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => resource.GetGenericType(name);
		public LsnType GetType(string name) => resource.GetType(name);
		public TypeId GetTypeId(string name) => resource.GetTypeId(name);
		public bool TypeExists(string name) => resource.TypeExists(name);

		readonly List<Field> Fields = new List<Field>();
		readonly List<ScriptClassMethod> AbstractMethods = new List<ScriptClassMethod>();

		public void RegisterField(string name, TypeId id, bool mutable)
		{
			Fields.Add(new Field(Fields.Count, name, id, mutable));
		}

		public void RegisterAbstractMethod(string name, TypeId returnType, IReadOnlyList<Parameter> parameters)
		{
			AbstractMethods.Add(new ScriptClassMethod(Id, returnType, parameters, Path, true, true, name));
		}

		public SymbolType CheckSymbol(string symbol)
		{
			if (Fields.Any(f => f.Name == symbol)) return SymbolType.Field;
			if (AbstractMethods.Any(m => m.Name == symbol)) return SymbolType.ScriptClassMethod;
			// ...

			return resource.Script.CheckSymbol(symbol);
		}
	}
}
