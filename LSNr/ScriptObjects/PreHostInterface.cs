using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LSNr.ReaderRules;

namespace LSNr
{
	public class PreHostInterface : IPreScript
	{
		private readonly string Name;

		public readonly TypeId HostInterfaceId;

		private readonly IPreScript Parent;

		private readonly IReadOnlyList<Token> Tokens; // The tokens between (and not including) the opening and closing braces.

		private readonly Dictionary<string, EventDefinition> Events = new Dictionary<string, EventDefinition>();

		private readonly Dictionary<string, FunctionSignature> Methods = new Dictionary<string, FunctionSignature>();

		public IScope CurrentScope { get; set; }

		public bool Mutable => Parent.Mutable;

		public bool Valid
		{
			get { return Parent.Valid; }
			set { Parent.Valid = value; }
		}

		public string Path => Parent.Path;

		internal PreHostInterface(string name, IPreScript parent, IReadOnlyList<Token> tokens)
		{
			Name = name; HostInterfaceId = new TypeId(name);  Parent = parent; Tokens = tokens;
		}

		public SymbolType CheckSymbol(string name)		=> Parent.CheckSymbol(name);
		public Function GetFunction(string name)		=> Parent.GetFunction(name);
		public bool GenericTypeExists(string name)		=> Parent.GenericTypeExists(name);
		public GenericType GetGenericType(string name)	=> Parent.GetGenericType(name);
		public LsnType GetType(string name)				=> Parent.GetType(name);
		public bool TypeExists(string name)				=> name == Name || Parent.TypeExists(name);

		public TypeId GetTypeId(string name)
		{
			if (name == Name) return HostInterfaceId;
			return Parent.GetTypeId(name);
		}

		internal HostInterfaceType Parse()
		{
			var i = 0;
			while (i < Tokens.Count)
			{
				try
				{
					var val = Tokens[i].Value;
					if (val == "event")
					{
						i++;
						var eventDef = ParseEventDefinition(ref i);
						Events.Add(eventDef.Name, eventDef);
					}
					else if (val == "fn")
					{
						i++;
						var methodDef = ParseMethodDefinition(ref i);
						Methods.Add(methodDef.Name, methodDef);
					}
					else
						throw LsnrParsingException.UnexpectedToken(Tokens[i], "'fn' or 'event'", Path);
				}
				catch (LsnrException e)
				{
					Valid = false;
					Logging.Log("host interface", Name, e);
					return null;
				}
				catch (Exception e)
				{
					Valid = false;
					Logging.Log("host interface", Name, e, Path);
					return null;
				}
			}

			var hostType = new HostInterfaceType(HostInterfaceId, Methods, Events);
			HostInterfaceId.Load(hostType);
			return hostType;
		}

		private List<Parameter> ParseParameters(IReadOnlyList<Token> tokens)
		{
			switch (Parent)
			{
				case ResourceBuilder resourceBuilder:
					return resourceBuilder.ParseParameters(tokens);
				case PreResource pre: return pre.ParseParameters(tokens);
				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="i">Initially, points to the first token in the definition (i.e. the name).</param>
		/// <returns></returns>
		private EventDefinition ParseEventDefinition(ref int i)
		{
			string name = Tokens[i].Value;
			i++; // 'i' should now point to the opening parenthesis.
			if (Tokens[i].Value != "(")
				throw new LsnrParsingException(Tokens[i - 1], $"Error parsing event definition '{name}': Expected '('.", Path);


			var paramTokens = new List<Token>();
			while (Tokens[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(Tokens[i]);

			var parameters = ParseParameters(paramTokens);

			i++; // 'i' Points to the thing after the closing parenthesis.
			if (i > Tokens.Count - 1 || Tokens[i].Value != ";")
				throw new LsnrParsingException(Tokens[i >= Tokens.Count ? i - 1 : i], $"Error parsing event definition '{name}': Expected ';'.", Path);
			i++; // 'i' now points to the thing after the ';'.

			return new EventDefinition(name,parameters);
		}

		private FunctionSignature ParseMethodDefinition(ref int i)
		{
			string name = Tokens[i].Value;
			i++; // 'i' should now point to the opening parenthesis.
			if (Tokens[i].Value != "(")
				throw new LsnrParsingException(Tokens[i], $"Error parsing method definition '{name}': Expected '('.", Path);

			var paramTokens = new List<Token>();
			while (Tokens[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(Tokens[i]);

			var parameters = ParseParameters(paramTokens);
			TypeId returnType = null;
			i++; // 'i' Points to the thing after the closing parenthesis.
			if (i > Tokens.Count - 1)
				throw new LsnrParsingException(Tokens[i - 1], $"Error parsing method definition '{name}': Expected ';' or '->'.", Path);

			if (Tokens[i].Value == "->")
			{
				i++; // 'i' points to the thing after '->'.
				if (Tokens[i].Value == "(")
				{
					if (Tokens[++i].Value != ")")
						throw new LsnrParsingException(Tokens[i], $"Error parsing method definition '{name}': Expected '('.", Path);
					// 'i' points to ')'.
					i++; // 'i' points to the thing after ')'.
				}
				else
				{
					int j = i;
					returnType = this.ParseTypeId(Tokens, i, out i); // i points to the thing after the return type.
					if (i < 0)
					{
						i = Tokens.Count;
						throw new LsnrParsingException(Tokens[j], $"Error parsing method definition '{name}': Failed to parse return type.", Path);
					}
				}
			}
			else if(Tokens[i].Value != ";")
				throw new LsnrParsingException(Tokens[i], $"Error parsing method definition '{name}': Expected ';', received '{Tokens[i].Value}'.", Path);

			if (i > Tokens.Count - 1 || Tokens[i].Value != ";")
				throw new LsnrParsingException(Tokens[i >= Tokens.Count ? i - 1 : i], $"Error parsing event definition '{name}': Expected ';'.", Path);
			i++; // 'i' points to the thing after ';'

			return new FunctionSignature(parameters, name, returnType);
		}


		public void GenericTypeUsed(TypeId typeId)
		{
			Parent.GenericTypeUsed(typeId);
		}
	}
}
