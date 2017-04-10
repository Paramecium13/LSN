using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using Tokens;

namespace LSNr
{
	// 1. Create TypeIds for HostInterfaces and ScriptObjects.
	// 2. Parse HostInterfaces.
	// 3. Parse ScriptObjects.

	public sealed class PreScriptObject : BasePreScriptObject
	{
		

		private readonly string Name;


		private readonly List<Property> Properties = new List<Property>();

		private readonly List<Field> Fields = new List<Field>();



		public override IScope CurrentScope { get; set; }

		public override bool Mutable => Resource.Mutable;

		public override bool Valid
		{
			get
			{
				return Resource.Valid;
			}
			set
			{
				Resource.Valid = value;
			}
		}

		
		private int DefaultStateIndex;


		public PreScriptObject(string name, PreResource resource, string hostName, IReadOnlyList<IToken> tokens):base(tokens, new TypeId(name),resource,hostName)
		{
			Name = name;
		}


		public override bool FunctionExists(string name) => Resource.FunctionExists(name);
		public override bool FunctionIsIncluded(string name) => Resource.FunctionIsIncluded(name);
		public override Function GetFunction(string name) => Resource.GetFunction(name);

		public override bool TypeExists(string name) => name == Name || Resource.TypeExists(name);
		public override LsnType GetType(string name) => Resource.GetType(name);

		public override bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);
		public override GenericType GetGenericType(string name) => Resource.GetGenericType(name);


		private bool PreParsed = false;

		public override SymbolType CheckSymbol(string name)
		{
			if (Methods.ContainsKey(name)) return SymbolType.ScriptObjectMethod;
			if (Fields.Any(f => f.Name == name)) return SymbolType.Field;
			if (Properties.Any(p => p.Name == name)) return SymbolType.Property;

			if (HostType.HasMethod(name)) return SymbolType.HostInterfaceMethod;

			return Resource.CheckSymbol(name);
		}

		public override TypeId GetTypeId(string name)
		{
			if (name == Name) return Id;
			return Resource.GetTypeId(name);
		}

		internal bool MethodExists(string name) => Methods.ContainsKey(name);


		internal ScriptObjectMethod GetMethod(string name) => Methods[name];


		internal int GetPropertyIndex(string name)
		{
			var prop = Properties.FirstOrDefault(p => p.Name == name);
			return Properties.IndexOf(prop);
		}

		internal Property GetProperty(string name) => Properties.FirstOrDefault(p => p.Name == name);


		internal Field GetField(string name) 
			=> Fields.First(f => f.Name == name);

		internal bool StateExists(string name) => false;


		internal int GetStateIndex(string name) => -1;

		protected override bool MethodSignatureValid(FunctionSignature signature) => !Methods.ContainsKey(signature.Name); // No method exists with this name.


		internal void PreParse()
		{
			bool defaultStateDefined = false;
			int previousStateIndex = -1;

			int i = 0;
			while (i < Tokens.Count)
			{
				var val = Tokens[i].Value;
				switch (val)
				{
					case "fn":
						i++;
						var fn = PreParseMethod(ref i);
						Methods.Add(fn.Name, fn);
						break;
					case "on":
						i++;
						var ev = PreParseEventListener(ref i);
						EventListeners.Add(ev.Definition.Name, ev);
						break;
					case "auto":
						if (defaultStateDefined)
							throw new ApplicationException();
						defaultStateDefined = true;
						i++;
						if (Tokens[i].Value != "state")
							throw new ApplicationException();
						goto case "state";
					case "state":
						throw new NotImplementedException();
					case "property":
						{
							i++;
							string metadata = null;
							LsnValue defaultVal = LsnValue.Nil;
							if(Tokens[i].Value == "<")
							{
								i++;
								metadata = Tokens[i].Value;
								i++;
								if (Tokens[i].Value != ">")
								{
									Valid = false;
									Console.WriteLine($"Error line {Tokens[i].LineNumber}: Unexpected token '{Tokens[i].Value}'. Expected '>'.");
									throw new ApplicationException("...");
								}
								else i++;
							}
							string name = Tokens[i].Value;
							i++;
							if (Tokens[i].Value != ":")
							{
								Valid = false;
								Console.WriteLine($"Error line {Tokens[i].LineNumber}: Unexpected token '{Tokens[i].Value}'. Expected ':'.");
							}
							else i++;
							var typeId = this.ParseTypeId(Tokens, i, out i);
							if (Tokens[i].Value == "=")
							{
								throw new NotImplementedException();
							}
							else if (Tokens[i].Value == ";")
								i++;
							else
							{
								Valid = false;
								Console.WriteLine($"Error line {Tokens[i].LineNumber}: Unexpected token '{Tokens[i].Value}'. Expected ';'.");
							}
						}
						break;
					default:
						if (!(i == Tokens.Count - 1 && val == "}"))
							throw new NotImplementedException("");
						i++;
						break;
				}
			}
			PreParsed = true;
		}

		
		private void ParseMethods()
		{
			foreach(var pair in MethodBodies)
			{
				var method = Methods[pair.Key];
				var pre = new PreScriptObjectFunction(this);
				foreach (var param in method.Parameters)
					pre.CurrentScope.CreateVariable(param);
				var parser = new Parser(pair.Value, pre);
				parser.Parse();
				pre.CurrentScope.Pop(parser.Components);
				method.Components = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
				method.StackSize = (pre.CurrentScope as VariableTable)?.MaxSize ?? -1;
			}
		}

		private void ParseEventListeners()
		{
			foreach(var pair in  EventListenerBodies)
			{
				var eventListener = EventListeners[pair.Key];
				var pre = new PreScriptObjectFunction(this);
				foreach (var param in eventListener.Definition.Parameters)
					pre.CurrentScope.CreateVariable(param);
				var parser = new Parser(pair.Value, pre);
				parser.Parse();
				pre.CurrentScope.Pop(parser.Components);
				eventListener.Components = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
				eventListener.StackSize = (pre.CurrentScope as VariableTable)?.MaxSize ?? -1;
			}
		}

		public ScriptObjectType Parse()
		{
			if (!PreParsed)
				throw new InvalidOperationException();
			// Parse methods
			ParseMethods();

			// Parse event listeners
			ParseEventListeners();

			// Parse states

			var states = new Dictionary<int, ScriptObjectState>(1);
			states.Add(DefaultStateIndex, new ScriptObjectState(0,new Dictionary<string,ScriptObjectMethod>(),new Dictionary<string,EventListener>()));
			var type = new ScriptObjectType(Id, HostType.Id, Properties, Fields, Methods, EventListeners,
				states, DefaultStateIndex);
			Id.Load(type);
			return type;
		}

	}
}
