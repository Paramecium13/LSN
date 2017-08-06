using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LSNr.Optimization;
using Tokens;

namespace LSNr
{
	public abstract class BasePreScriptObject : IPreScript
	{

		public abstract IScope CurrentScope { get; set; }
		public abstract bool Mutable { get; }
		public abstract bool Valid { get; set; }

		// The Id of the ScriptObject this is for.
		internal readonly TypeId Id;
		protected readonly IReadOnlyList<Token> Tokens;
		protected readonly PreResource Resource;

		internal readonly string HostName;
		internal HostInterfaceType HostType;

		protected readonly Dictionary<string, ScriptObjectMethod> Methods = new Dictionary<string, ScriptObjectMethod>();
		protected readonly Dictionary<string, IReadOnlyList<Token>> MethodBodies = new Dictionary<string, IReadOnlyList<Token>>();
		protected readonly Dictionary<string, EventListener> EventListeners = new Dictionary<string, EventListener>();
		protected readonly Dictionary<string, IReadOnlyList<Token>> EventListenerBodies = new Dictionary<string, IReadOnlyList<Token>>();

		public abstract SymbolType CheckSymbol(string name);
		public abstract bool FunctionExists(string name);
		public abstract bool FunctionIsIncluded(string name);
		public abstract bool GenericTypeExists(string name);
		public abstract Function GetFunction(string name);
		internal abstract Field GetField(string name);
		public abstract GenericType GetGenericType(string name);
		public abstract LsnType GetType(string name);
		public abstract TypeId GetTypeId(string name);
		public abstract bool TypeExists(string name);

		internal abstract Property GetProperty(string val);
		internal abstract int GetPropertyIndex(string val);
		internal abstract bool StateExists(string name);
		internal abstract int GetStateIndex(string name);

		protected BasePreScriptObject(IReadOnlyList<Token> tokens, TypeId id, PreResource resource, string hostName)
		{
			Tokens = tokens; Id = id; Resource = resource; HostName = hostName;
		}



		public abstract bool IsMethodSignatureValid(FunctionSignature signature);

		internal bool HostEventExists(string name) => HostType?.HasEventDefinition(name) ?? false;
		internal EventDefinition GetHostEventDefinition(string name) => HostType?.GetEventDefinition(name);

		internal bool HostMethodExists(string name) => HostType?.HasMethod(name) ?? false; //ToDo: Use...
		internal FunctionSignature GetHostMethodSignature(string name) => HostType?.GetMethodDefinition(name); //ToDo: Use...


		private List<Parameter> ParseParameters(IReadOnlyList<Token> tokens)
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

		protected ScriptObjectMethod PreParseMethod(ref int i) // 'i' points to the thing after 'fn'.
		{
			var val = Tokens[i].Value;
			bool isAbstract = false;
			bool isVirtual = false;
			if (val == "abstract")
			{
				isAbstract = true;
				isVirtual = true;
				i++;
			}
			else if(val == "virtual")
			{
				isVirtual = true;
				i++;
			}
			// 'i' points to the name.
			var name = Tokens[i].Value;
			i++;
			if (Tokens[i].Value != "(")
				throw new ApplicationException("...");

			var paramTokens = new List<Token>();
			while (Tokens[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(Tokens[i]);


			var preParameters = ParseParameters(paramTokens);

			var parameters = new List<Parameter> { new Parameter("self", Id, LsnValue.Nil, 0) };
			parameters.AddRange(preParameters.Select(p => new Parameter(p.Name, p.Type, p.DefaultValue, (ushort)(p.Index + 1))));

			TypeId returnType = null;
			i++; // 'i' Points to the thing after the closing parenthesis.
			if (i > Tokens.Count - 1)
			{
				Valid = false;
				Console.WriteLine($"Error line {Tokens[Tokens.Count - 1].LineNumber}: Expected ';', '->', or '{{'");
				throw new ApplicationException();
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
					returnType = this.ParseTypeId(Tokens, i, out i); // 'i' points to the thing after the return type.
					if (i < 0)
					{
						i = Tokens.Count;
						throw new ApplicationException("Critical error parsing return type...");
					}
				}
			}

			if (Tokens[i].Value == ";")
			{
				if (!isAbstract)
				{
					Valid = false;
					Console.WriteLine($"Error line {Tokens[i].LineNumber}: A non abstract method must declare a body.");
				}
			}
			else if (Tokens[i].Value == "{")
			{
				if(isAbstract)
				{
					Console.WriteLine($"Error line {Tokens[i].LineNumber}: An abstract method may not declare a body.");
					Valid = false;
					return new ScriptObjectMethod(Id, returnType, parameters, Resource.Environment, isVirtual, isAbstract, name);
				}
				/*if (i > Tokens.Count - 1)
				{
					Console.WriteLine($"Error line {Tokens[i].LineNumber}: Unexpected end of file.");
					Valid = false;
					return new ScriptObjectMethod(Id, returnType, parameters, Resource.Environment, isVirtual, isAbstract, name);
				}*/

				var tokens = new List<Token>();
				int openCount = 1;
				while (openCount > 0)
				{
					i++;
					
					var v = Tokens[i].Value;
					if (v == "{")
						openCount++;					
					else if (v == "}")
						openCount--;

					if (openCount > 0) tokens.Add(Tokens[i]);
				}
				MethodBodies.Add(name, tokens);
			}
			else
			{
				var x = isAbstract ? ", ';'," : "";
				Console.WriteLine($"Error line {Tokens[i].LineNumber}: Unexpected token '{Tokens[i].Value}'. Expected '->'{x} or '{{'. ");
			}

			i++; // 'i' points to the thing after ';' or the end of the body.

			return new ScriptObjectMethod(Id, returnType, parameters, Resource.Environment, isVirtual, isAbstract, name);
		}


		protected EventListener PreParseEventListener(ref int i)
		{
			var name = Tokens[i].Value;
			if(!HostEventExists(name))
			{
				Console.WriteLine($"Error line {Tokens[i].LineNumber}: The HostInterface '{HostType.Name}' does not define an event '{name}'.");
				throw new ApplicationException("...");
			}

			i++; // 'i' should point to '('.
			if(Tokens[i].Value != "(")
			{
				Valid = false;
				Console.WriteLine($"Error line {Tokens[i].LineNumber}: Expected '('. Received '{Tokens[i].Value}'.");
				throw new ApplicationException();
			}

			var paramTokens = new List<Token>();
			while (Tokens[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(Tokens[i]);

			//var parameters = ParseParameters(paramTokens);


			var preParameters = ParseParameters(paramTokens);

			var parameters = new List<Parameter> { new Parameter("self", Id, LsnValue.Nil, 0) };
			parameters.AddRange(preParameters.Select(p => new Parameter(p.Name, p.Type, p.DefaultValue, (ushort)(p.Index + 1))));

			i++;// 'i' Points to the thing after the closing parenthesis.
			if (i > Tokens.Count - 1)
			{
				Valid = false;
				Console.WriteLine($"Error line {Tokens[Tokens.Count - 1].LineNumber}: Expected '{{'");
				throw new ApplicationException();
			}

			if (Tokens[i].Value != "{")
			{
				throw new ApplicationException("...");
			}

			var def = new EventDefinition(name, parameters);
			var preDef = new EventDefinition(name, preParameters);

			var hostDef = GetHostEventDefinition(name);
			if (!hostDef.Equivalent(preDef))
			{
				Console.WriteLine($"Error line {Tokens[i].Value}: the event '{name}' does not match the event definition in the HostInterface '{HostType.Name}'.");
				throw new ApplicationException("");
			}

			var tokens = new List<Token>();
			int openCount = 1;
			while (openCount > 0)
			{
				i++;

				var v = Tokens[i].Value;
				if (v == "{")
					openCount++;
				else if (v == "}")
					openCount--;

				if (openCount > 0) tokens.Add(Tokens[i]);
			}
			EventListenerBodies.Add(name, tokens);

			// Use def, which contains a self parameter, instead of hostDef, which doesn't.
			return new EventListener(def, Resource.Environment);
		}

		protected void ParseMethods()
		{
			foreach (var pair in MethodBodies)
			{
				var method = Methods[pair.Key];
				var pre = new PreScriptObjectFunction(this);
				foreach (var param in method.Parameters)
					pre.CurrentScope.CreateVariable(param);
				var parser = new Parser(pair.Value, pre);
				parser.Parse();
				pre.CurrentScope.Pop(parser.Components);

				var components = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
				method.Code = new ComponentFlattener().Flatten(components);
				method.StackSize = (pre.CurrentScope as VariableTable)?.MaxSize + 1 /*For the 'self' arg.*/?? -1;
			}
		}

		protected void ParseEventListeners()
		{
			foreach (var pair in EventListenerBodies)
			{
				var eventListener = EventListeners[pair.Key];
				var pre = new PreScriptObjectFunction(this);
				
				foreach (var param in eventListener.Definition.Parameters)
					pre.CurrentScope.CreateVariable(param);
				var parser = new Parser(pair.Value, pre);
				parser.Parse();
				pre.CurrentScope.Pop(parser.Components);
				eventListener.Code = new ComponentFlattener().Flatten(Parser.Consolidate(parser.Components).Where(c => c != null).ToList());
				eventListener.StackSize = (pre.CurrentScope as VariableTable)?.MaxSize + 1 /*For the 'self' arg.*/?? -1;
			}
		}

		public bool TypeIsIncluded(TypeId type)
		{
			return Resource.TypeIsIncluded(type);
		}
	}
}
