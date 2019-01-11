using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.ScriptObjects
{
	public sealed class HostInterfaceComponent : IPreHostInterface
	{
		readonly ITypeContainer TypeContainer;
		readonly TypeId Id;
		readonly ISlice<Token> Tokens;
		readonly List<EventDefinition> Events = new List<EventDefinition>();
		readonly List<FunctionSignature> Methods = new List<FunctionSignature>();
		public string Path { get; }

		public HostInterfaceComponent(ITypeContainer typeContainer, TypeId id, ISlice<Token> tokens, string path)
		{
			TypeContainer = typeContainer; Id = id; Tokens = tokens; Path = path;
		}

		public bool GenericTypeExists(string name) => TypeContainer.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => TypeContainer.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => TypeContainer.GetGenericType(name);

		public LsnType GetType(string name) => name != Id.Name ? TypeContainer.GetType(name)
			: throw new ApplicationException();

		public TypeId GetTypeId(string name) => name == Id.Name ? Id : TypeContainer.GetTypeId(name);
		public bool TypeExists(string name) => TypeContainer.TypeExists(name) || name == Id.Name;

		public void RegisterEvent(EventDefinition ev) { Events.Add(ev); }

		public void RegisterMethod(FunctionSignature fn) { Methods.Add(fn); }

		public void OnParsingSignatures(IPreResource resource)
		{
			var reader = new HostInterfaceReader(Tokens, this);
			reader.Read();
			var t = new HostInterfaceType(Id, Methods.ToDictionary(m => m.Name), Events.ToDictionary(e => e.Name));
			resource.RegisterHostInterface(t);
		}
	}
}
