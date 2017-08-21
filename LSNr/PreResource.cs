using LsnCore;
using LsnCore.Types;
using LSNr.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tokens;

namespace LSNr
{
	public class PreResource : BasePreScript
	{
		internal const string STRN = "Στρ";
		internal const string SUBN = "SUB";


		private IScope _CurrentScope = new VariableTable(new List<Variable>());

		public override IScope CurrentScope { get { return _CurrentScope; } set { _CurrentScope = value; } }

		//private readonly Dictionary<Identifier, List<IToken>> InlineLiterals = new Dictionary<Identifier, List<IToken>>();

		private readonly Dictionary<string, LsnStructType> StructTypes = new Dictionary<string, LsnStructType>();

		private readonly Dictionary<string, RecordType> RecordTypes = new Dictionary<string, RecordType>();
		


		private readonly Dictionary<string, LsnFunction> MyFunctions = new Dictionary<string, LsnFunction>();

		private readonly Dictionary<string, List<Token>> FunctionBodies = new Dictionary<string, List<Token>>();


		//private readonly List<GenericType> GenericTypes = LsnType.GetBaseGenerics();

		//private readonly Dictionary<string, IReadOnlyList<IToken>> HostInterfaceBodies = new Dictionary<string, IReadOnlyList<IToken>>();
		private readonly Dictionary<string, PreHostInterface> PreHostInterfaces = new Dictionary<string, PreHostInterface>();

		private readonly Dictionary<string, PreScriptObject> PreScriptObjects = new Dictionary<string, PreScriptObject>();


		public PreResource(string src, string path) : base(src,path){}

		/// <summary>
		/// Reifies the source...
		/// </summary>
		public void Reify()
		{
			ProcessDirectives();
			Tokenize();
			PreParseFunctions(PreParseTypes());
			ParseHostInterfaces();
			PreParseScriptObjects();
			foreach (var pre in PreScriptObjects.Values)
				pre.Parse();
			ParseFunctions();
        }
		

		/// <summary>
		/// 
		/// </summary>
		public void ProcessDirectives() { Text = ProcessDirectives(Source); }

		
		

		/// <summary>
		/// Go through the source, parsing structs and records.
		/// </summary>
		/// <returns> Tokens that are not part of a struct or record.</returns>
		private IReadOnlyList<Token> PreParseTypes()
		{
			var otherTokens = new List<Token>();
			for(int i = 0; i < Tokens.Count; i++)
			{
				var val = Tokens[i].Value;
				if(val == "struct")
				{
					string name = Tokens[++i].Value; // Move on to the next token, get the name.
					if (Tokens[++i].Value != "{") // Move on to the next token, make sure it is '{'.
					{
						Console.WriteLine($"Error in parsing struct {name}: invalid token: {Tokens[i]}, expected {{.");
						Valid = false;
					}
					else ++i; // Move on to the token after '{'.
					var tokens = new List<Token>();
					while (Tokens[i].Value != "}") tokens.Add(Tokens[i++]);
					MakeStruct(name, tokens);
				}
				else if(val == "record")
				{
					string name = Tokens[++i].Value; // Move on to the next token, get the name.
					if (Tokens[++i].Value != "{") // Move on to the next token, make sure it is '{'.
					{
						Console.WriteLine($"Error in parsing record {name}: invalid token: {Tokens[i]}, expected {{.");
						Valid = false;
					}
					else ++i; // Move on to the token after '{'.
					var tokens = new List<Token>();
					while (Tokens[i].Value != "}") tokens.Add(Tokens[i++]);
					MakeRecord(name, tokens);
				}
				else if (val == "hostinterface")
				{
					string name = Tokens[++i].Value; // Move on to the next token, get the name.
					if (Tokens[++i].Value != "{") // Move on to the next token, make sure it is '{'.
					{
						Console.WriteLine($"Error in parsing HostInterface {name}: invalid token: {Tokens[i]}, expected {{.");
						Valid = false;
					}
					else ++i; // Move on to the token after '{'.
					var tokens = new List<Token>();
					while (Tokens[i].Value != "}") tokens.Add(Tokens[i++]);
					PreHostInterfaces.Add(name, new PreHostInterface(name, this, tokens));
				}
				else if (val == "unique" || val == "scriptobject")
				{
					bool unique = false;
					if(val == "unique")
					{
						unique = true;
						i++;
					}
					string name = Tokens[++i].Value; // Move on to the next token, get the name.
					string hostName = null;
					i++;// Move on to the next token...
					if (Tokens[i].Value == "<")
					{
						i++;
						hostName = Tokens[i].Value;
						i++;
					}

					if (Tokens[i].Value != "{")
						throw new ApplicationException("");

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
					var sc = new PreScriptObject(name, this, hostName, unique, tokens);
					PreScriptObjects.Add(name, sc);
				}
				else otherTokens.Add(Tokens[i]);
			}
			return otherTokens;
		}

		// Parse Functions:
		//	* Go through source and extract names, parameters, return types, and bodies.
		//		* Use the names, parameters, and return types to create a(n) LSN_Function and store it in Functions.
		//		* Store the name and body in a Dictionary<string,List<IToken>> named FunctionBodies.
		//	* Go through FunctionBodies and parse the tokens.
		//		* Put the resulting List<Component> in the LSN_Function of the same name stored in Functions.
		private List<Token> PreParseFunctions(IReadOnlyList<Token> tokens)
		{
			var otherTokens = new List<Token>();
			for (int i = 0; i < tokens.Count; i++)
			{
				if (tokens[i].Value == "fn")
				{
					string name = tokens[++i].Value;
					if(tokens[++i].Value != "(")
					{
						Console.WriteLine($"Error in parsing function {name} expected token '(', received '{tokens[i].Value}'.");
						Valid = false; continue;
					}
					var paramTokens = new List<Token>();
					while (tokens[++i].Value != ")") // This starts with the token after '('.
						paramTokens.Add(tokens[i]);
					List<Parameter> paramaters = null;
					//try
					//{
						paramaters = ParseParameters(paramTokens);
					/*}
					catch (Exception e)
					{
						Console.WriteLine($"Error in parsing parameters of function {name}");
						Console.WriteLine("\t" + e.Message);
						Valid = false; continue;
					}*/
					// At this point, the current token (i.e. tokens[i].Value) is ')'.
					TypeId returnType = null;
					if (tokens[++i].Value == "->")
					{
						if(tokens[++i].Value == "(")
						{ // The current token is the token after '->'.
							if(tokens[++i].Value != ")")
							{
								Console.WriteLine($"Error in parsing function {name} expected token '(', received '{tokens[i].Value}'.");
								Valid = false; continue;
							}
						}
						else
						{ // The current token is the token after '->'.
							try
							{
								returnType = this.ParseTypeId(tokens, i, out i);
							}
							catch (Exception e)
							{
								Console.WriteLine($"Error in parsing return type of function {name}.");
								Console.WriteLine("\t" + e.Message);
								Valid = false; continue;
							}
						}
					}
					if(tokens[i].Value != "{")
					{
						Console.WriteLine($"Error in parsing function {name} expected token '{{', received '{tokens[i].Value}'.");
						Valid = false; continue;
					}
					var fnBody = new List<Token>();
					int openCount = 1;
					int closeCount = 0;
					string v = null;
					while (true)
					{
						v = tokens[++i].Value;
						if(v == "{") openCount++;
						else if (v == "}")
						{
							closeCount++;
							if (closeCount == openCount) break;
						}
						fnBody.Add(tokens[i]);
					}
					var fn = new LsnFunction(paramaters, returnType, name, Environment);
					IncludeFunction(fn);
					FunctionBodies.Add(name, fnBody);
					MyFunctions.Add(name,fn);
				}
				else otherTokens.Add(tokens[i]);
				
			}
			return otherTokens;
		}

		/// <summary>
		/// 
		/// </summary>
		private void ParseFunctions()
		{
			foreach(var pair in MyFunctions)
			{
				var preFn = new PreFunction(this);
				foreach (var param in pair.Value.Parameters)
					preFn.CurrentScope.CreateVariable(param);
				var parser = new Parser(FunctionBodies[pair.Key], preFn);
				parser.Parse();
				preFn.CurrentScope.Pop(parser.Components);
				pair.Value.Code = new ComponentFlattener().Flatten(Parser.Consolidate(parser.Components).Where(c => c != null).ToList());
				pair.Value.StackSize = (preFn.CurrentScope as VariableTable)?.MaxSize?? -1;		
			}
		}
		

		private void ParseHostInterfaces()
		{
			foreach(var pre in PreHostInterfaces.Values)
			{
				var host = pre.Parse();
				HostInterfaces.Add(host.Name, host);
			}
		}


		private void PreParseScriptObjects()
		{
			foreach (var pre in PreScriptObjects.Values)
			{
				if(pre.HostName != null)
				{
					if (!TypeExists(pre.HostName))
						throw new ApplicationException("...");
					pre.HostType = HostInterfaces[pre.HostName];
				}
				var sc = pre.PreParse();
				ScriptObjects.Add(sc.Name, sc);
			}
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		private List<Parameter> ParseParameters(IReadOnlyList<Token> tokens)
		{
			var paramaters = new List<Parameter>();
			ushort index = 0;
			for(int i = 0; i < tokens.Count; i++)
			{
				string name = tokens[i].Value;
				if(tokens[++i].Value != ":")
					throw new ApplicationException($"Error: Expected token ':' after parameter name {name} received token '{tokens[i].Value}'.");
                TypeId type = this.ParseTypeId(tokens, ++i, out i);
				LsnValue defaultValue = LsnValue.Nil;
				if (i < tokens.Count && tokens[i].Value == "=")
				{
					if (tokens[++i].Type == TokenType.String)
					{
						if (type != LsnType.string_.Id)
							throw new ApplicationException($"Error in parsing parameter {name}: cannot assign a default value of type string to a parameter of type {type.Name}");
						defaultValue = new LsnValue (new StringValue(tokens[i].Value));
						if (i + 1 < tokens.Count) i++;
					}
					else if (tokens[i].Type == TokenType.Integer)
					{
						if (type != LsnType.int_.Id)
						{
							if (type == LsnType.double_.Id)
							{
								defaultValue = new LsnValue(tokens[i].IntValue );
							}
							else
								throw new ApplicationException($"Error in parsing parameter {name}: cannot assign a default value of type int to a parameter of type {type.Name}");
						}
						else defaultValue = new LsnValue(tokens[i].IntValue);
						if (i + 1 < tokens.Count) i++;
					}
					else if (tokens[i].Type == TokenType.Float)
					{
						if (type != LsnType.double_.Id)
							throw new ApplicationException($"Error in parsing parameter {name}: cannot assign a default value of type double to a parameter of type {type.Name}");
						defaultValue = new LsnValue(tokens[i].DoubleValue);
						if(i + 1 < tokens.Count) i++;
					}
					// Bools and other stuff...
					else throw new ApplicationException($"Error in parsing default value for parameter {name}.");
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
		/// <param name="name"></param>
		/// <param name="typeOfType">struct or record.</param>
		/// <param name="tokens"></param>
		/// <returns></returns>
		private Dictionary<string, LsnType> ParseFields(string name, string typeOfType, IReadOnlyList<Token> tokens)
		{
			if (tokens.Count < 3) // struct Circle { Radius : double}
			{
				Console.WriteLine($"Error, invalid {typeOfType} {name}.");
				Valid = false;
				return null;
			}
			var fields = new Dictionary<string, LsnType>();
			for (int i = 0; i < tokens.Count; i++)
			{
				string fName = tokens[i++].Value; // Get the name of the field, move on to the next token.
				if (i >= tokens.Count) // Make sure the definition does not end.
				{
					Valid = false;
					Console.WriteLine($"Error in {typeOfType} {name}: unexpected end of declaration, expected \':\'.");
					return null;
				}
				if (tokens[i++].Value != ":") // Make sure the next token is ':', move on to the next token.
				{
					Valid = false;
					Console.WriteLine($"Error in {typeOfType} {name}: unexpected token {tokens[i - 1].Value}, expected \':\'");
					return null;
				}
				if (i >= tokens.Count) // Make sure the definition does not end.
				{
					Valid = false;
					Console.WriteLine($"Error in {typeOfType} {name}: unexpected end of declaration, expected type.");
					return null;
				}
				LsnType type = this.ParseType(tokens, i, out i);
				fields.Add(fName, type);
				if (i + 1 < tokens.Count && tokens[i].Value == ",") // Check if the definition ends, move on to the next token
																	  // and check that it is ','.
				{
					// Move on to the next token, which should be the name of the next token.
					continue; // Move on to the next field.
				}
				else break;
			}
			return fields;
		}

		/// <summary>
		/// Make a struct.
		/// </summary>
		/// <param name="name"> The name of the struct to make.</param>
		/// <param name="tokens"> The tokens defining the struct.</param>
		private void MakeStruct(string name, IReadOnlyList<Token> tokens)
		{
			Dictionary<string, LsnType> fields = null;
			try
			{
				fields = ParseFields(name, "struct", tokens);
            }
			catch (Exception e)
			{
				Console.WriteLine($"Error in parsing struct {name}.");
				if (true/*Show exeption info*/)
					Console.WriteLine(e.Message);
			}
			if (fields == null) return;
			var structType = new LsnStructType(name, fields);
            StructTypes.Add(name, structType);
			IncludedTypes.Add(structType);
		}

		/// <summary>
		/// Make a record.
		/// </summary>
		/// <param name="name"> The name of the record.</param>
		/// <param name="tokens"> The tokens defining the record.</param>
		private void MakeRecord(string name, IReadOnlyList<Token> tokens)
		{
			Dictionary<string, LsnType> fields = null;
			try
			{
				fields = ParseFields(name, "record", tokens);
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in parsing struct {name}.");
				if (true/*ShowExeptionInfo*/)
					Console.WriteLine(e.Message);
			}
			if (fields == null) return;
			var recordType = new RecordType(name, fields);
			RecordTypes.Add(name, recordType);
			IncludedTypes.Add(recordType);
		}

		public LsnResourceThing GetResource()
		{
			return new LsnResourceThing()
			{
				Functions = IncludedFunctions,
				Includes = Includes,
				RecordTypes = RecordTypes,
				StructTypes = StructTypes,
				Usings = Usings,
				HostInterfaces = HostInterfaces,
				ScriptObjectTypes = ScriptObjects
				//TODO: Add IncludedTypes.
			};
		}


		public override bool TypeExists(string name) => PreScriptObjects.ContainsKey(name) || PreHostInterfaces.ContainsKey(name) || base.TypeExists(name);


		public override TypeId GetTypeId(string name)
		{
			if (PreScriptObjects.ContainsKey(name))
				return PreScriptObjects[name].Id;
			if (PreHostInterfaces.ContainsKey(name))
				return PreHostInterfaces[name].HostInterfaceId;
			return base.GetTypeId(name);
		}

		public override SymbolType CheckSymbol(string name)
		{
			if (FunctionExists(name))
				return SymbolType.Function;
			if (_CurrentScope.VariableExists(name))
				return SymbolType.Variable;
			if (UniqueScriptObjectTypeExists(name))
				return SymbolType.UniqueScriptObject;

			return SymbolType.Undefined;
		}

		public override bool UniqueScriptObjectTypeExists(string name)
			=> base.UniqueScriptObjectTypeExists(name) || PreScriptObjects.Any(p => p.Key == name && p.Value.IsUnique);
		
	}
}
