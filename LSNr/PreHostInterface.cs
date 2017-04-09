using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using Tokens;
using Tokens.Tokens;

namespace LSNr
{
	public class PreHostInterface : IPreScript
	{
		private readonly string Name;

		public readonly TypeId HostInterfaceId;

		private readonly BasePreScript Parent;

		private readonly IReadOnlyList<IToken> Tokens; // The tokens between (and not including) the opening and closing braces.

		private readonly Dictionary<string, EventDefinition> Events = new Dictionary<string, EventDefinition>();

		private readonly Dictionary<string, FunctionSignature> Methods = new Dictionary<string, FunctionSignature>();

		public IScope CurrentScope { get; set; }

		public bool Mutable => Parent.Mutable;

		public bool Valid
		{
			get { return Parent.Valid; }
			set { Parent.Valid = value; }
		}


		internal PreHostInterface(string name, BasePreScript parent, IReadOnlyList<IToken> tokens)
		{
			Name = name; HostInterfaceId = new TypeId(name);  Parent = parent; Tokens = tokens;
		}


		public SymbolType CheckSymbol(string name) => Parent.CheckSymbol(name);

		public bool FunctionExists(string name)
		{
			throw new NotImplementedException();
		}

		public bool FunctionIsIncluded(string name)
		{
			throw new NotImplementedException();
		}

		public Function GetFunction(string name)
		{
			throw new NotImplementedException();
		}

		public bool GenericTypeExists(string name) => Parent.GenericTypeExists(name);


		public GenericType GetGenericType(string name) => Parent.GetGenericType(name);

		public LsnType GetType(string name) => Parent.GetType(name);

		public bool TypeExists(string name) => name == Name || Parent.TypeExists(name);

		public TypeId GetTypeId(string name)
		{
			if (name == Name) return HostInterfaceId;
			return Parent.GetTypeId(name);
		}

		
		internal HostInterfaceType Parse()
		{
			int i = 0;
			while (i < Tokens.Count)
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
				else throw new ApplicationException("Unexpected token...");
			}


			var hostType = new HostInterfaceType(HostInterfaceId, Methods, Events);
			HostInterfaceId.Load(hostType);
			return hostType;
		}

		private List<Parameter> ParseParameters(IReadOnlyList<IToken> tokens)
		{
			var paramaters = new List<Parameter>();
			ushort index = 0;
			for (int i = 0; i < tokens.Count; i++)
			{
				string name = tokens[i].Value;
				if (tokens[++i].Value != ":")
					throw new ApplicationException($"Error: Expected token ':' after parameter name {name} received token '{tokens[i].Value}'.");
				var type = this.ParseTypeId(tokens, ++i, out i);
				LsnValue defaultValue = LsnValue.Nil;
				if (i < tokens.Count && tokens[i].Value == "=")
				{
					Valid = false;
					Console.Write($"Error line {tokens[i].LineNumber}: Cannot have default values for host interface methods or events.");
					i++;
				}
				paramaters.Add(new Parameter(name, type, defaultValue, index++));
				if (i < tokens.Count && tokens[i].Value != ",")
					throw new ApplicationException($"Error: expected token ',' after definition of parameter {name}, received '{tokens[i].Value}'.");
			}
			return paramaters;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i">Initially, points to the first token in the definition (i.e. the name).</param>
		/// <returns></returns>
		private EventDefinition ParseEventDefinition(ref int i)
		{
			string name = Tokens[i].Value;
			i++; // 'i' should now point to the opening parenthesis.
			if (Tokens[i].Value != "(")
				throw new ApplicationException("...");

			var paramTokens = new List<IToken>();
			while (Tokens[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(Tokens[i]);

			var parameters = ParseParameters(paramTokens);

			i++; // 'i' Points to the thing after the closing parenthesis.
			if (i > Tokens.Count - 1)
			{
				Valid = false;
				Console.WriteLine($"Error line {Tokens[Tokens.Count - 1].LineNumber}: Expected ';'");
				return new EventDefinition(name, parameters);
			}

			if (Tokens[i].Value != ";")
			{
				Valid = false;
				Console.WriteLine($"Error line {Tokens[Tokens.Count - 1].LineNumber}: Expected ';'");
				return new EventDefinition(name, parameters);
			}
			i++; // 'i' now points to the thing after the ';'.

			return new EventDefinition(name,parameters);
		}


		private FunctionSignature ParseMethodDefinition(ref int i)
		{
			string name = Tokens[i].Value;
			i++; // 'i' should now point to the opening parenthesis.
			if (Tokens[i].Value != "(")
				throw new ApplicationException("...");

			var paramTokens = new List<IToken>();
			while (Tokens[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(Tokens[i]);

			var parameters = ParseParameters(paramTokens);
			TypeId returnType = null;
			i++; // 'i' Points to the thing after the closing parenthesis.
			if (i > Tokens.Count - 1)
			{
				Valid = false;
				Console.WriteLine($"Error line {Tokens[Tokens.Count - 1].LineNumber}: Expected ';' or '->'");
				return new FunctionSignature(parameters, name, returnType);
			}

			if (Tokens[i].Value == "->")
			{
				i++; // 'i' points to the thing after '->'.
				if (Tokens[i].Value == "(")
				{
					if (Tokens[++i].Value != ")")
					{
						Valid = false;
						Console.WriteLine($"Error line {Tokens[i].LineNumber}: Expected ')'");
						throw new ApplicationException();
					} // 'i' points to ')'.
					i++; // 'i' points to the thing after ')'.
				}
				else
				{
					returnType = this.ParseTypeId(Tokens, i, out i); // i points to the thing after the return type.
					if (i < 0)
					{
						i = Tokens.Count;
						throw new ApplicationException("Critical error parsing return type...");
					}
				}
			}
			else throw new ApplicationException("Unexpected token...");

			if (i > Tokens.Count - 1)
			{
				Valid = false;
				Console.WriteLine($"Error line {Tokens[Tokens.Count - 1].LineNumber}: Expected ';'");
				return new FunctionSignature(parameters, name, returnType);
			}

			if (Tokens[i].Value != ";")
			{
				Valid = false;
				Console.WriteLine("...");
			}
			i++; // 'i' points to the thing after ';'

			return new FunctionSignature(parameters, name, returnType);
		}
		



	}
}
