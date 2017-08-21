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

		internal readonly bool IsUnique;

		private readonly IList<PreState> PreStates = new List<PreState>();

		private readonly List<Property> Properties = new List<Property>();

		private readonly List<Field> Fields = new List<Field>();

		private int DefaultStateIndex = -1;

		public override IScope CurrentScope { get; set; }

		public override bool Mutable => Resource.Mutable;

		public override bool Valid
		{
			get{ return Resource.Valid; }
			set{ Resource.Valid = value; }
		}

		public PreScriptObject(string name, PreResource resource, string hostName, bool isUnique, IReadOnlyList<Token> tokens)
			:base(tokens, new TypeId(name),resource,hostName)
		{
			Name = name; IsUnique = isUnique;
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
			if (HostType != null && HostType.HasMethod(name)) return SymbolType.HostInterfaceMethod;
			if (name == Name && IsUnique) return SymbolType.UniqueScriptObject;
			
			return Resource.CheckSymbol(name);
		}

		public override TypeId GetTypeId(string name)
		{
			if (name == Name) return Id;
			return Resource.GetTypeId(name);
		}

		internal bool MethodExists(string name) => Methods.ContainsKey(name); //ToDo: Use...


		internal ScriptObjectMethod GetMethod(string name) => Methods[name]; //ToDo: Use...


		internal override int GetPropertyIndex(string name)
		{
			var prop = Properties.Find(p => p.Name == name);
			return Properties.IndexOf(prop);
		}

		internal override Property GetProperty(string name) => Properties.Find(p => p.Name == name);


		internal override Field GetField(string name)
			=> Fields.First(f => f.Name == name);

		internal override bool StateExists(string name) => PreStates.Any(p => p.StateName == name);


		internal override int GetStateIndex(string name) => PreStates.FirstOrDefault(p => p.StateName == name).Index;


		/// <summary>
		/// No method with this name has been defined already.
		/// </summary>
		/// <param name="signature"></param>
		/// <returns></returns>
		public override bool IsMethodSignatureValid(FunctionSignature signature) => !Methods.ContainsKey(signature.Name); // No method exists with this name.

		// ToDo: Make this return a ScriptObjectType
		internal ScriptObjectType PreParse()
		{
			bool defaultStateDefined = false;
			bool isDefaultState = false;
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
						try
						{
							i++; // Looking at token after 'state'
							if (Tokens[i].Type != TokenType.Identifier)
								throw new ApplicationException();
							var stateName = Tokens[i].Value;
							i++; // Looking at token after the state name.
							var v = Tokens[i].Value;
							if (v == "{")
								previousStateIndex++; // This is the current index.
							else if(v == ":" || v == "=")
							{
								i++; // Looking at token after the association indicator (i.e. ':' or '=').
								if (Tokens[i].Type == TokenType.Integer)
									previousStateIndex = Tokens[i].IntValue;
								else
									throw new ApplicationException($"Error line {Tokens[i].LineNumber}: Unexpected token '{Tokens[i].Value}'. Expected integer token.");
							}
							else
								throw new ApplicationException($"Error line {Tokens[i].LineNumber}: Unexpected token '{v}'. Expected '{{', ':', or '='.");

							var tokens = new List<Token>();
							int openCount = 1;
							while (openCount > 0)
							{
								i++;

								var v1 = Tokens[i].Value;
								if (v1 == "{")
									openCount++;
								else if (v1 == "}")
									openCount--;

								if (openCount > 0) tokens.Add(Tokens[i]);
							}

							if(isDefaultState)
							{
								DefaultStateIndex = previousStateIndex;
								isDefaultState = false;
							}

							PreStates.Add( new PreState(this,stateName,previousStateIndex,Resource,tokens));
						}
						catch (ApplicationException e) { throw e; }
						catch (Exception e)
						{
							if (i >= Tokens.Count)
								throw new ApplicationException($"Error Line {Tokens[Tokens.Count-1]}: Unexpected end of file.");
							else
								throw new ApplicationException($"Error Line {Tokens[i].LineNumber}: Unspecified state preparsing error.",e);
						}
						break;
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
							Properties.Add(new Property(name, typeId, defaultVal, metadata));
						}
						break;
					case "mut":
						{
							i++;
							string name = Tokens[i].Value;
							if (Properties.Any(p => p.Name == name) || Fields.Any(f => f.Name == name))
								throw new ApplicationException($"Name already in use...");
							i++;
							LsnType type;
							if (Tokens[i].Value == ":")
							{
								i++;
								type = GetType(Tokens[i].Value);
								i++; // 'i' should point to ';'
							}
							else if (Tokens[i].Value == "=")
							{
								throw new ApplicationException("...");
								/*i++;
								var ex = Tokens.Skip(i).TakeWhile(t => t.Value != ";").ToList();
								var expr = Create.Express(ex, this);
								i += ex.Count; // 'i' should point to ';'*/
							}
							else
								throw new ApplicationException();
							if (Tokens[i].Value != ";")
								throw new ApplicationException();
							i++;
							Fields.Add(new Field(Fields.Count, name, type));
						}
						break;
					default:
						/*if (!(i == Tokens.Count - 1 && val == "}"))
							throw new NotImplementedException("");*/
						i++;
						break;
				}
			}
			PreParsed = true;

			// PreParse states
			var states = PreStates.Select(p => p.PreParse()).ToDictionary((s) => s.Id);
			var scObjType = new ScriptObjectType(Id, HostType?.Id, Properties, Fields, Methods, EventListeners, states, DefaultStateIndex, IsUnique);
			Id.Load(scObjType);
			return scObjType;
		}

		internal void Parse()
		{
			if (!PreParsed) throw new InvalidOperationException();
			// Parse methods
			ParseMethods();

			// Parse event listeners
			ParseEventListeners();

			// Parse states
			foreach (var state in PreStates) state.Parse();
		}

	}
}
