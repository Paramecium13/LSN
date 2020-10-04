using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;

namespace LSNr.ScriptObjects
{
	public sealed class StateBuilder : IPreState
	{
		readonly IPreScriptClass ScriptClass;
		readonly bool Auto;
		readonly string Name;

		public string Path => ScriptClass.Path;
		public bool Valid { get => ScriptClass.Valid; set => ScriptClass.Valid = value; }
		public TypeId Id => ScriptClass.Id;
		public TypeId HostId => ScriptClass.HostId;
		public HostInterfaceType Host => ScriptClass.Host;
		public SymbolType CheckSymbol(string symbol) => ScriptClass.CheckSymbol(symbol);
		public bool GenericTypeExists(string name) => ScriptClass.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => ScriptClass.GenericTypeUsed(typeId);
		public Function GetFunction(string name) => ScriptClass.GetFunction(name);
		public GenericType GetGenericType(string name) => ScriptClass.GetGenericType(name);
		public LsnType GetType(string name) => ScriptClass.GetType(name);
		public TypeId GetTypeId(string name) => ScriptClass.GetTypeId(name);
		public bool TypeExists(string name) => ScriptClass.TypeExists(name);
		public int GetStateIndex(string name) => ScriptClass.GetStateIndex(name);
		public Field GetField(string val) => ScriptClass.GetField(val);
		public bool StateExists(string stateName) => ScriptClass.StateExists(stateName);
		public bool MethodExists(string value) => ScriptClass.MethodExists(value);
		public IReadOnlyList<Parameter> ParseParameters(IReadOnlyList<Token> tokens) => ScriptClass.ParseParameters(tokens);

		readonly List<ScriptClassMethod> Methods = new List<ScriptClassMethod>();
		readonly List<EventListener> EventListeners = new List<EventListener>();
		ScriptClassState State;

		public event Action<IPreState> ParsingProcBodies;
		public event Action<IPreState> ParsingSignaturesB;


		public StateBuilder(IPreScriptClass scriptClass, string name, bool auto)
		{
			ScriptClass = scriptClass; Name = name; Auto = auto;
		}

		public void PreParse(ISlice<Token> tokens)
		{
			var reader = new StateReader(tokens, this);
			reader.Read();
		}

		public void OnParsingStateSignatures(IPreScriptClass pre)
		{
			if (pre != ScriptClass)
				throw new ApplicationException();
			ParsingSignaturesB?.Invoke(this);
			State = ScriptClass.RegisterState(Name, Auto, Methods, EventListeners);
		}

		public void OnParsingProcBodies(IPreScriptClass pre)
		{
			if (pre != ScriptClass)
				throw new ApplicationException();
			ParsingProcBodies?.Invoke(this);
		}

		/// <inheritdoc/>
		public ScriptClassMethod RegisterMethod(string name, TypeId returnType, IReadOnlyList<Parameter> parameters)
		{
			var m = new ScriptClassMethod(Id, returnType, parameters, Path, false, false, name);
			Methods.Add(m);
			return m;
		}

		public IProcedure CreateFunction(IReadOnlyList<Parameter> args, TypeId retType, string name, bool isVirtual = false)
			=> RegisterMethod(name, retType, args);

		public EventListener RegisterEventListener(string name, IReadOnlyList<Parameter> parameters)
		{
			var e = new EventListener(new EventDefinition(name, parameters), Path);
			EventListeners.Add(e);
			return e;
		}

	}
}
