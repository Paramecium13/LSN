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
		readonly IFunctionContainer Resource;
		readonly TypeId Id;
		readonly ISlice<Token> Tokens;
		readonly List<EventDefinition> Events = new List<EventDefinition>();
		readonly List<FunctionSignature> Methods = new List<FunctionSignature>();
		public string Path { get; }
		public bool Valid { get => Resource.Valid; set => Resource.Valid = value; }

		public HostInterfaceComponent(IFunctionContainer typeContainer, TypeId id, ISlice<Token> tokens, string path)
		{
			Resource = typeContainer; Id = id; Tokens = tokens; Path = path;
		}

		public bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Resource.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Resource.GetGenericType(name);

		public LsnType GetType(string name) => name != Id.Name ? Resource.GetType(name)
			: throw new ApplicationException();
		public Function GetFunction(string name) => Resource.GetFunction(name);
		public SymbolType CheckSymbol(string symbol) => Resource.CheckSymbol(symbol);

		public TypeId GetTypeId(string name) => name == Id.Name ? Id : Resource.GetTypeId(name);
		public bool TypeExists(string name) => Resource.TypeExists(name) || name == Id.Name;

		public void RegisterEvent(EventDefinition ev) { Events.Add(ev); }

		public void RegisterMethod(FunctionSignature fn) { Methods.Add(fn); }

		public void OnParsingSignatures(IPreResource resource)
		{
			var reader = new HostInterfaceReader(Tokens, this);
			reader.Read();
			var t = new HostInterfaceType(Id, Methods.ToDictionary(m => m.Name), Events.ToDictionary(e => e.Name));
			resource.RegisterHostInterface(t);
		}

		public IReadOnlyList<Parameter> ParseParameters(IReadOnlyList<Token> tokens) => Resource.ParseParameters(tokens);

		public IProcedure CreateFunction(IReadOnlyList<Parameter> args, TypeId retType, string name, bool isVirtual = false)
		{
			throw new NotImplementedException();
		}
	}
}
