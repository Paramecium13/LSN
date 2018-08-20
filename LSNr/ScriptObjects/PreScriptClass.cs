using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LSNr.Optimization;

namespace LSNr
{
	// 1. Create TypeIds for HostInterfaces and ScriptObjects.
	// 2. Parse HostInterfaces.
	// 3. Parse ScriptObjects.

	public sealed class PreScriptClass : BasePreScriptClass
	{
		internal readonly string Name;

		internal readonly bool IsUnique;

		private readonly IList<PreState> PreStates = new List<PreState>();

		private readonly List<Property> Properties = new List<Property>();

		private readonly List<Field> Fields = new List<Field>();

		private int DefaultStateIndex = -1;

		private ScriptClassConstructor Constructor;

		private List<Token> ConstructorTokens;

		public override IScope CurrentScope { get; set; }

		public override bool Mutable => Resource.Mutable;

		public override bool Valid
		{
			get{ return Resource.Valid; }
			set{ Resource.Valid = value; }
		}

		public PreScriptClass(string name, PreResource resource, string hostName, bool isUnique, IReadOnlyList<Token> tokens)
			:base(tokens, new TypeId(name),resource,hostName)
		{
			Name = name; IsUnique = isUnique; //ToDo: Make the typeId contain the actual type...
		}

		public override bool FunctionExists(string name)		=> Resource.FunctionExists(name);
		public override Function GetFunction(string name)		=> Resource.GetFunction(name);
		public override bool GenericTypeExists(string name)		=> Resource.GenericTypeExists(name);
		public override GenericType GetGenericType(string name)	=> Resource.GetGenericType(name);
		public override LsnType GetType(string name)			=> name == Name ? Id.Type : Resource.GetType(name);
		public override TypeId GetTypeId(string name)			=> name == Name ? Id : Resource.GetTypeId(name);
		public override bool TypeExists(string name)			=> name == Name || Resource.TypeExists(name);

		public override SymbolType CheckSymbol(string name)
		{
			if (MethodExists(name)) return SymbolType.ScriptClassMethod;
			if (Fields.Any(f => f.Name == name)) return SymbolType.Field;
			if (Properties.Any(p => p.Name == name)) return SymbolType.Property;
			if (HostMethodExists(name)) return SymbolType.HostInterfaceMethod;
			if (name == Name && IsUnique) return SymbolType.UniqueScriptObject;

			return Resource.CheckSymbol(name);
		}

		internal override int GetPropertyIndex(string name)
		{
			var prop = Properties.Find(p => p.Name == name);
			return Properties.IndexOf(prop);
		}

		internal override Property GetProperty(string name) => Properties.Find(p => p.Name == name);
		internal override Field GetField(string name)		=> Fields.First(f => f.Name == name);
		internal override bool StateExists(string name)		=> PreStates.Any(p => p.StateName == name);
		internal override int GetStateIndex(string name)	=> PreStates.FirstOrDefault(p => p.StateName == name).Index;

		/// <summary>
		/// No method with this name has been defined already.
		/// </summary>
		/// <param name="signature"></param>
		/// <returns></returns>
		public override bool IsMethodSignatureValid(FunctionSignature signature) =>
			CheckSymbol(signature.Name) == SymbolType.Undefined && !MethodExists(signature.Name); // No method exists with this name.

		internal ScriptClass PreParse()
		{
			var defaultStateDefined = false;
			var isDefaultState = false;
			var previousStateIndex = -1;

			var i = 0;
			while (i < Tokens.Count)
			{
				try
				{
					switch (Tokens[i].Value)
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
							i++;
							if (defaultStateDefined)
								throw new LsnrParsingException(Tokens[i-1], $"A default state is already defined for script object '{Id.Name}'.", Path);
							defaultStateDefined = true;
							if (Tokens[i].Value != "state")
								throw LsnrParsingException.UnexpectedToken(Tokens[i], "state", Path);
							goto case "state";
						case "state":
							{
								i++; // Looking at token after 'state'
								if (Tokens[i].Type != TokenType.Identifier)
									throw new LsnrParsingException(Tokens[i], $"{Tokens[i].Value} is not a valid state name.", Path);
								var stateName = Tokens[i].Value;
								//TODO: Make sute stateName isn't already an identifier for something else.
								i++; // Looking at token after the state name.
								var v = Tokens[i].Value;
								if (v == "{")
									previousStateIndex++; // This is the current index.
								else if (v == ":" || v == "=")
								{
									i++; // Looking at token after the association indicator (i.e. ':' or '=').
									if (Tokens[i].Type == TokenType.Integer)
										previousStateIndex = Tokens[i].IntValue;
									else
										throw LsnrParsingException.UnexpectedToken(Tokens[i], "integer token", Path);
								}
								else
									throw LsnrParsingException.UnexpectedToken(Tokens[i], "'{{', ':', or '='", Path);

								var tokens = new List<Token>();
								var openCount = 1;
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

								if (isDefaultState)
								{
									DefaultStateIndex = previousStateIndex;
									isDefaultState = false;
								}

								PreStates.Add(new PreState(this, stateName, previousStateIndex, Resource, tokens));
							}
							break;
						case "property":
							{
								i++;
								string metadata = null;
								if (Tokens[i].Value == "<")
								{
									i++;
									metadata = Tokens[i].Value;
									i++;
									if (Tokens[i].Value != ">")
										throw LsnrParsingException.UnexpectedToken(Tokens[i], "'>'", Path);
									i++;
								}
								var name = Tokens[i].Value;
								i++;
								if (Tokens[i].Value != ":")
									throw LsnrParsingException.UnexpectedToken(Tokens[i], "':'", Path);
								i++;
								var typeId = this.ParseTypeId(Tokens, i, out i);
								if (Tokens[i].Value == "=")
								{
									throw new LsnrParsingException(Tokens[i], "Properties cannot have default values.", Path);
								}
								if (Tokens[i].Value == ";")
									i++;
								else
									throw LsnrParsingException.UnexpectedToken(Tokens[i], "';'", Path);
								Properties.Add(new Property(name, typeId, metadata));
							}
							break;
						case "mut":
							{
								i++;
								var name = Tokens[i].Value;
								if (CheckSymbol(name) != SymbolType.Undefined || Properties.Any(p => p.Name == name) || Fields.Any(f => f.Name == name))
									throw new LsnrParsingException(Tokens[i], $"The identifier '{name}' is already used for something else.", Path);
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
									throw new NotImplementedException();
									/*i++;
									var ex = Tokens.Skip(i).TakeWhile(t => t.Value != ";").ToList();
									var expr = Create.Express(ex, this);
									i += ex.Count; // 'i' should point to ';'*/
								}
								else
									throw new LsnrParsingException(Tokens[i], $"Must declare a type for the field '{name}'.", Path);
								if (Tokens[i].Value != ";")
									throw LsnrParsingException.UnexpectedToken(Tokens[i], "';'", Path);
								i++;
								Fields.Add(new Field(Fields.Count, name, type));
							}
							break;
						case "new":
							{
								if (Constructor != null)
									throw new LsnrParsingException(Tokens[i], $"Error parsing Script Class '{Name}': Cannot have more than one constructor.", Path);
								i++;
								if (Tokens[i].Value != "(")
									throw new LsnrParsingException(Tokens[i], $"Error parsing constructor for '{Name}': expected '(', received '{Tokens[i].Value}'.", Path);

								var paramTokens = new List<Token>();
								while (Tokens[++i].Value != ")") // This starts with the token after '('.
									paramTokens.Add(Tokens[i]);

								var preParameters = ParseParameters(paramTokens, false);

								var parameters = new List<Parameter> { new Parameter("self", Id, LsnValue.Nil, 0) };
								parameters.AddRange(preParameters.Select(p => new Parameter(p.Name, p.Type, p.DefaultValue, (ushort)(p.Index + 1))));
								i++; // 'i' Points to the thing after the closing parenthesis.
								Constructor = new ScriptClassConstructor(Path, parameters.ToArray());
								if (Tokens[i].Value != "{")
									throw LsnrParsingException.UnexpectedToken(Tokens[i], "{", Path);
								ConstructorTokens = new List<Token>();
								int openCount = 1;
								while (openCount > 0)
								{
									i++;
									var v = Tokens[i].Value;
									if (v == "{") openCount++;
									else if (v == "}") openCount--;
									if (openCount > 0) ConstructorTokens.Add(Tokens[i]);
								}
								break;
							}
						default:
							/*if (!(i == Tokens.Count - 1 && val == "}"))
								throw new NotImplementedException("");*/
							i++;
							break;
					}
				}
				catch (LsnrException e)
				{
					Logging.Log("script object", Name, e);
					Valid = false;
				}
				catch (Exception e)
				{
					Logging.Log("script object", Name, e, Path);
					Valid = false;
				}
			}

			// PreParse states
			var states = PreStates.Select(p => p.PreParse()).ToDictionary((s) => s.Id);
			var scClass = new ScriptClass(Id, HostType?.Id, Properties, Fields, Methods, EventListeners, states, DefaultStateIndex, IsUnique);
			Id.Load(scClass);
			return scClass;
		}

		internal bool Parse()
		{
			// Parse methods
			ParseMethods();

			// Parse event listeners
			ParseEventListeners();

			// Parse states
			foreach (var state in PreStates)
				Valid &= state.Parse();

			if (Constructor != null)
				ParseConstructor();

			return Valid;
		}

		private void ParseConstructor()
		{
			try
			{
				var pre = new PreScriptClassFunction(this);
				foreach (var param in Constructor.Parameters)
					pre.CurrentScope.CreateVariable(param);
				var parser = new Parser(ConstructorTokens, pre);
				parser.Parse();
				pre.CurrentScope.Pop(parser.Components);

				var components = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
				Constructor.Code = new ComponentFlattener().Flatten(components);
				Constructor.StackSize = (pre.CurrentScope as VariableTable)?.MaxSize + 1 /*For the 'self' arg.*/?? -1;
			}
			catch (LsnrException e)
			{
				Valid = false;
				Logging.Log($"constructor for script class {Name}", e);
			}
			catch (Exception e)
			{
				Valid = false;
				Logging.Log($"consructor for script class {Name}", e, Path);
			}
		}

		public override FunctionSignature GetMethodSignature(string name)
			=> Methods[name].Signature;
	}
}
