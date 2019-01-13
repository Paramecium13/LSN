using LsnCore;
using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.ScriptObjects
{
	public sealed class ScriptClassBuilder : IPreScriptClass
	{
		readonly ITypeContainer typeContainer;

		public bool GenericTypeExists(string name) => typeContainer.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => typeContainer.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => typeContainer.GetGenericType(name);
		public LsnType GetType(string name) => typeContainer.GetType(name);
		public TypeId GetTypeId(string name) => typeContainer.GetTypeId(name);
		public bool TypeExists(string name) => typeContainer.TypeExists(name);
	}
}
