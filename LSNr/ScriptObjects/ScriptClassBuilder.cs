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
	public sealed class ScriptClassBuilder : IPreScriptClass
	{
		readonly IPreResource Resource;
		public TypeId Id { get; }
		private readonly string HostName;
		public TypeId HostId { get; private set; }
		public HostInterfaceType Host { get; private set; }
		readonly ISlice<Token> Tokens;
		readonly bool Unique;
		readonly string Metadata;

		public bool Valid { get => Resource.Valid; set { Resource.Valid = value; } }
		public string Path => Resource.Path;
		public bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Resource.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Resource.GetGenericType(name);
		public LsnType GetType(string name) => Resource.GetType(name);
		public TypeId GetTypeId(string name) => Resource.GetTypeId(name);
		public bool TypeExists(string name) => Resource.TypeExists(name);
		public Function GetFunction(string name) => Resource.GetFunction(name);

		readonly List<Field> Fields = new List<Field>();
		readonly List<ScriptClassMethod> AbstractMethods = new List<ScriptClassMethod>();
		readonly List<ScriptClassMethod> NonAbstractMethods = new List<ScriptClassMethod>();
		readonly List<EventListener> EventListeners = new List<EventListener>();
		readonly Dictionary<string, ScriptClassState> States = new Dictionary<string, ScriptClassState>();
		int? First;
		ScriptClassConstructor Constructor;

		public event Action<IPreScriptClass> ParsingProcBodies;
		public event Action<IPreScriptClass> ParsingSignaturesB;
		public event Action<IPreScriptClass> ParsingStateSignatures;

		public ScriptClassBuilder(IPreResource resource, string name, string host, bool unique, string metadata, ISlice<Token> tokens)
		{
			Resource = resource; Id = new TypeId(name); HostName = host; Unique = unique; Metadata = metadata; Tokens = tokens;
		}

		public void OnParsingSignaturesB(IPreResource pre)
		{
			var reader = new ScriptClassReader(Tokens, this);
			reader.Read();
			if (pre != Resource)
				throw new ApplicationException();
			if(HostName != null)
			{
				HostId = Resource.GetTypeId(HostName);
				Host = (HostInterfaceType)HostId.Type;
			}
			ParsingSignaturesB?.Invoke(this);
			ParsingStateSignatures?.Invoke(this);
			var methods = AbstractMethods.Union(NonAbstractMethods)
				.ToDictionary(m => m.Name);
			Resource.RegisterScriptClass(new ScriptClass(Id, HostId, new List<Property>(), Fields, methods,
				EventListeners.ToDictionary(e => e.Definition.Name), States.Values.ToDictionary(s => s.Id), First ?? 0, Unique, Metadata, Constructor));
		}

		public void OnParsingProcBodies(IPreResource pre)
		{
			if (pre != Resource)
				throw new ApplicationException();
			ParsingProcBodies?.Invoke(this);
		}

		public void RegisterField(string name, TypeId id, bool mutable)
		{
			Fields.Add(new Field(Fields.Count, name, id, mutable));
		}

		public void RegisterAbstractMethod(string name, TypeId returnType, IReadOnlyList<Parameter> parameters)
		{
			AbstractMethods.Add(new ScriptClassMethod(Id, returnType, parameters, Path, true, true, name));
		}

		public ScriptClassMethod RegisterMethod(string name, TypeId returnType, IReadOnlyList<Parameter> parameters, bool isVirtual)
		{
			if (NonAbstractMethods.Any(x => x.Name == name) || AbstractMethods.Any(x => x.Name == name))
				throw new ApplicationException("...");
			var m = new ScriptClassMethod(Id, returnType, parameters, Path, isVirtual, false, name);
			NonAbstractMethods.Add(m);
			return m;
		}

		public EventListener RegisterEventListener(string name, IReadOnlyList<Parameter> parameters)
		{
			if (EventListeners.Any(l => l.Definition.Name == name))
				throw new ApplicationException();
			var e = new EventListener(new EventDefinition(name, parameters), Path);
			EventListeners.Add(e);
			return e;
		}

		public ScriptClassConstructor RegisterConstructor(IReadOnlyList<Parameter> parameters)
		{
			if (Constructor != null) return null;
			var c = new ScriptClassConstructor(Path, parameters.ToArray());
			Constructor = c;
			return c;
		}

		public ScriptClassState RegisterState(string name, bool auto, IReadOnlyList<ScriptClassMethod> methods, IReadOnlyList<EventListener> eventListeners)
		{
			if (States.ContainsKey(name))
				throw new ApplicationException("...");
			var mdict = methods.ToDictionary(m => m.Name);
			var st = new ScriptClassState(States.Count, mdict, eventListeners.ToDictionary(e => e.Definition.Name));
			if (auto)
			{
				if (First != null)
					throw new ApplicationException("...");
				First = st.Id;
			}
			var c = 0;
			foreach (var pair in mdict)
			{
				if (AbstractMethods.Any(m => m.Name == pair.Key))
				{
					var m = AbstractMethods.First(x => x.Name == pair.Key);
					if (!m.Signature.Equals(pair.Value.Signature))
						throw new ApplicationException("...");
					c++;
					continue;
				}
				if(NonAbstractMethods.Any(m => m.Name == pair.Key))
				{
					var m = NonAbstractMethods.First(x => x.Name == pair.Key);
					if(!m.IsVirtual)
						throw new ApplicationException("Not virtual...");
					if (!m.Signature.Equals(pair.Value.Signature))
						throw new ApplicationException("...");
					continue;
				}
				throw new ApplicationException("...");
			}
			if (c != AbstractMethods.Count)
				throw new ApplicationException("Not all abstract methods implemented...");
			States.Add(name, st);
			return st;
		}

		public SymbolType CheckSymbol(string symbol)
		{
			if (Fields.Any(f => f.Name == symbol)) return SymbolType.Field;
			if (AbstractMethods.Any(m => m.Name == symbol) || NonAbstractMethods.Any(m => m.Name == symbol))
				return SymbolType.ScriptClassMethod;
			// ...

			return Resource.Script.CheckSymbol(symbol);
		}

		public int GetStateIndex(string name) => States[name].Id;

		public Field GetField(string val) => Fields.First(f => f.Name == val);

		public bool StateExists(string stateName) => States.ContainsKey(stateName);

		public bool MethodExists(string value) => AbstractMethods.Any(m => m.Name == value) || NonAbstractMethods.Any(m => m.Name == value);
	}
}
