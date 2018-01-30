﻿using LsnCore;
using LsnCore.Types;
using LSNr.Optimization;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace LSNr
{
	public class PreResource : BasePreScript
	{
		internal const string STRN = "Στρ";
		internal const string SUBN = "SUB";

		// Relative to '\src'
		public readonly string RelativePath;

		private IScope _CurrentScope = new VariableTable(new List<Variable>());

		public override IScope CurrentScope { get { return _CurrentScope; } set { _CurrentScope = value; } }

		//private readonly Dictionary<Identifier, List<IToken>> InlineLiterals = new Dictionary<Identifier, List<IToken>>();

		private readonly Dictionary<string, RecordType> RecordTypes = new Dictionary<string, RecordType>();

		private readonly Dictionary<string, StructType> StructTypes = new Dictionary<string, StructType>();

		private readonly Dictionary<string, LsnFunction> MyFunctions = new Dictionary<string, LsnFunction>();

		private readonly Dictionary<string, List<Token>> FunctionBodies = new Dictionary<string, List<Token>>();

		//private readonly List<GenericType> GenericTypes = LsnType.GetBaseGenerics();

		//private readonly Dictionary<string, IReadOnlyList<IToken>> HostInterfaceBodies = new Dictionary<string, IReadOnlyList<IToken>>();
		private readonly Dictionary<string, PreHostInterface> PreHostInterfaces = new Dictionary<string, PreHostInterface>();

		private readonly Dictionary<string, PreScriptClass> PreScriptClasses = new Dictionary<string, PreScriptClass>();


		public PreResource(string src, string path) : base(src,path)
		{
			RelativePath = new string(path.Skip(4).ToArray());
		}

		/// <summary>
		/// Reifies the source...
		/// </summary>
		public void Reify()
		{
			/*try
			{*/
				ProcessDirectives();
			/*}
			catch (Exception)
			{
				//TODO: Logging
				throw;
			}*/

			try
			{
				Tokenize();
			}
			catch (Exception)
			{
				//TODO: Logging
				throw;
			}

			PreParseFunctions(PreParseTypes());
			ParseHostInterfaces();
			PreParseScriptObjects();

			foreach (var pre in PreScriptClasses.Values)
			{
				try
				{
					Valid &= pre.Parse();
				}
				catch (LsnrException e)
				{
					Logging.Log("script object", pre.Id.Name, e);
					Valid = false;
				}
				catch (Exception e)
				{
					Logging.Log("script object", pre.Id.Name, e, Path);
					Valid = false;
				}
			}
			ParseFunctions();
		}

		/// <summary>
		/// ...
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
				if(val == "record")
				{
					var name = Tokens[++i].Value; // Move on to the next token, get the name.
												  //TODO : validate name.
					if (Tokens[++i].Value != "{") // Move on to the next token, make sure it is '{'.
					{
						Console.WriteLine($"Error in parsing struct {name}: invalid token: {Tokens[i]}, expected {{.");
						Valid = false;
					}
					else ++i; // Move on to the token after '{'.
					var tokens = new List<Token>();
					while (Tokens[i].Value != "}") tokens.Add(Tokens[i++]);
					MakeRecord(name, tokens);
				}
				else if(val == "struct")
				{
					var name = Tokens[++i].Value; // Move on to the next token, get the name.
												  //TODO : validate name.
					if (Tokens[++i].Value != "{") // Move on to the next token, make sure it is '{'.
					{
						Console.WriteLine($"Error in parsing record {name}: invalid token: {Tokens[i]}, expected {{.");
						Valid = false;
					}
					else ++i; // Move on to the token after '{'.
					var tokens = new List<Token>();
					while (Tokens[i].Value != "}") tokens.Add(Tokens[i++]);
					MakeStruct(name, tokens);
				}
				else if (val == "hostinterface")
				{
					var name = Tokens[++i].Value; // Move on to the next token, get the name.
												  //TODO : validate name.
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
				else if (val == "unique" || val == "scriptclass" || (val == "script" && Tokens[i+1].Value == "class"))
				{
					var unique = false;
					if (val == "unique")
					{
						unique = true;
						i++; // 'i' points to "script" or "scriptclass"
					}

					if (Tokens[i].Value.StartsWith("script", StringComparison.Ordinal))
					{
						if (Tokens[i].Value == "scriptclass")
						{
							i++; // 'i' points to the name.
						}
						else if (Tokens[i].Value == "script")
						{
							i++; // 'i' points to "class".
							if (Tokens[i].Value != "class")
								throw LsnrParsingException.UnexpectedToken(Tokens[i], "class", Path);
							i++; // 'i' points to the name.
						}
						else throw LsnrParsingException.UnexpectedToken(Tokens[i], "scriptclass or script class", Path);
					}
					else throw LsnrParsingException.UnexpectedToken(Tokens[i], "scriptclass or script class", Path);

					var name = Tokens[i].Value; // Move on to the next token, get the name.
												  //TODO : validate name.
					string hostName = null;
					i++;// Move on to the next token...
					if (Tokens[i].Value == "<")
					{
						i++; // 'i' points to host name.
						hostName = Tokens[i].Value;
						i++;
					}

					if (Tokens[i].Value != "{")
						throw LsnrParsingException.UnexpectedToken(Tokens[i],"{",Path);

					var tokens = new List<Token>();
					var openCount = 1;
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
					var sc = new PreScriptClass(name, this, hostName, unique, tokens);
					PreScriptClasses.Add(name, sc);
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
			string name = "";
			var otherTokens = new List<Token>();
			for (int i = 0; i < tokens.Count; i++)
			{
				if (tokens[i].Value == "fn")
				{
					try
					{
						var fnToken = tokens[i];
						name = tokens[++i].Value;
						//TODO : validate name.
						if (tokens[++i].Value != "(")
							throw LsnrParsingException.UnexpectedToken(tokens[i], "(", Path);
						var paramTokens = new List<Token>();
						while (tokens[++i].Value != ")") // This starts with the token after '('.
							paramTokens.Add(tokens[i]);
						var paramaters = ParseParameters(paramTokens);
						// At this point, the current token (i.e. tokens[i].Value) is ')'.
						TypeId returnType = null;
						if (tokens[++i].Value == "->")
						{
							if (tokens[++i].Value == "(")
							{ // The current token is the token after '->'.
								if (tokens[++i].Value != ")")
									throw LsnrParsingException.UnexpectedToken(tokens[i], ")", Path);
							}
							else
							{ // The current token is the token after '->'.
								try
								{
									returnType = this.ParseTypeId(tokens, i, out i);
								}
								catch (Exception e)
								{
									throw new LsnrParsingException(fnToken, "error parsing return type.", e,Path);
								}
								if (i < 0) throw new LsnrParsingException(fnToken, "error parsing return type.", Path);
							}
						}
						if (tokens[i].Value != "{")
							throw LsnrParsingException.UnexpectedToken(tokens[i], "}", Path);

						var fnBody = new List<Token>();
						int openCount = 1;
						int closeCount = 0;
						string v = null;
						while (true)
						{
							v = tokens[++i].Value;
							if (v == "{") openCount++;
							else if (v == "}")
							{
								closeCount++;
								if (closeCount == openCount) break;
							}
							fnBody.Add(tokens[i]);
						}
						var fn = new LsnFunction(paramaters, returnType, name, RelativePath);
						IncludeFunction(fn);
						FunctionBodies.Add(name, fnBody);
						MyFunctions.Add(name, fn);
					}
					catch (LsnrException e)
					{
						Logging.Log("function", name, e);
						Valid = false;
						continue;
					}
					catch (Exception e)
					{
						Logging.Log("function", name, e, Path);
						Valid = false;
						continue;
					}
				}
				else otherTokens.Add(tokens[i]);
			}
			return otherTokens;
		}

		private void ParseFunctions()
		{
			foreach(var pair in MyFunctions)
			{
				try
				{
					var preFn = new PreFunction(this);
					foreach (var param in pair.Value.Parameters)
						preFn.CurrentScope.CreateVariable(param);
					var parser = new Parser(FunctionBodies[pair.Key], preFn);
					parser.Parse();
					preFn.CurrentScope.Pop(parser.Components);
					if (preFn.Valid)
					{
						pair.Value.Code = new ComponentFlattener().Flatten(Parser.Consolidate(parser.Components).Where(c => c != null).ToList());
						pair.Value.StackSize = (preFn.CurrentScope as VariableTable)?.MaxSize ?? -1;
					}
					else
						Valid = false;
				}
				catch (LsnrException e)
				{
					Logging.Log("function", pair.Key, e);
					Valid = false;
				}
				catch (Exception e)
				{
					Logging.Log("function", pair.Key, e, Path);
				}
			}
		}

		private void ParseHostInterfaces()
		{
			foreach(var pre in PreHostInterfaces.Values)
			{
				try
				{
					var host = pre.Parse();
					if (pre.Valid)
					{
						HostInterfaces.Add(host.Name, host);
						MyHostInterfaces.Add(host.Name, host);
					}
					else Valid = false;
				}
				catch (LsnrException e)
				{
					Logging.Log("host interface", pre.HostInterfaceId.Name, e);
					Valid = false;
				}
				catch (Exception e)
				{
					Logging.Log("host interface", pre.HostInterfaceId.Name, e, Path);
					Valid = false;
				}
			}
		}

		private void PreParseScriptObjects()
		{
			foreach (var pre in PreScriptClasses.Values)
			{
				try
				{
					if (pre.HostName != null)
					{
						if (!TypeExists(pre.HostName))
							throw new LsnrTypeNotFoundException(Path, pre.HostName);
						pre.HostType = HostInterfaces[pre.HostName];
					}
					var sc = pre.PreParse();
					ScriptObjects.Add(sc.Name, sc);
				}
				catch (LsnrException e)
				{
					Logging.Log("script object", pre.Id.Name, e);
					Valid = false;
				}
				catch (Exception e)
				{
					Logging.Log($"script object '{pre.Id.Name}'",e,Path);
					Valid = false;
				}
			}
		}

		/// <summary>
		/// ...
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
				if (tokens[++i].Value != ":")
					throw new LsnrParsingException(tokens[i], $"Expected token ':' after parameter name {name} received token '{tokens[i].Value}'.", Path);
                TypeId type = this.ParseTypeId(tokens, ++i, out i);
				LsnValue defaultValue = LsnValue.Nil;
				if (i < tokens.Count && tokens[i].Value == "=")
				{
					if (tokens[++i].Type == TokenType.String)
					{
						if (type != LsnType.string_.Id)
							throw new LsnrParsingException(tokens[i],$"Error in parsing parameter {name}: cannot assign a default value of type string to a parameter of type {type.Name}",Path);
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
								throw new LsnrParsingException(tokens[i],$"Error in parsing parameter {name}: cannot assign a default value of type int to a parameter of type {type.Name}",Path);
						}
						else defaultValue = new LsnValue(tokens[i].IntValue);
						if (i + 1 < tokens.Count) i++;
					}
					else if (tokens[i].Type == TokenType.Float)
					{
						if (type != LsnType.double_.Id)
							throw new LsnrParsingException(tokens[i], $"Error in parsing parameter {name}: cannot assign a default value of type double to a parameter of type {type.Name}", Path);
						defaultValue = new LsnValue(tokens[i].DoubleValue);
						if(i + 1 < tokens.Count) i++;
					}
					// Bools and other stuff...
					else throw new LsnrParsingException(tokens[i], $"Error in parsing default value for parameter {name}.", Path);
				}
				paramaters.Add(new Parameter(name, type, defaultValue, index++));
				if (i < tokens.Count && tokens[i].Value != ",")
					throw new LsnrParsingException(tokens[i], $"expected token ',' after definition of parameter {name}, received '{tokens[i].Value}'.", Path);
			}
			return paramaters;
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="name"></param>
		/// <param name="typeOfType">struct or record.</param>
		/// <param name="tokens"></param>
		/// <returns></returns>
		private Tuple<string, TypeId>[] ParseFields(IReadOnlyList<Token> tokens)
		{
			if (tokens.Count < 3) // struct Circle { Radius : double}
			{
				throw new LsnrParsingException(tokens[0], "too few tokens.", Path);
			}
			var fields = new List<Tuple<string, TypeId>>();
			for (int i = 0; i < tokens.Count; i++)
			{
				string fName = tokens[i++].Value; // Get the name of the field, move on to the next token.
				if (i >= tokens.Count) // Make sure the definition does not end..
					throw new LsnrParsingException(tokens[i - 1], "unexpected end of declaration, expected ':'.", Path);
				if (tokens[i++].Value != ":") // Make sure the next token is ':', move on to the next token.
					throw LsnrParsingException.UnexpectedToken(tokens[i - 1], ":", Path);
				if (i >= tokens.Count) // Make sure the definition does not end.
					throw new LsnrParsingException(tokens[i - 1], "unexpected end of declaration, expected type.", Path);
				LsnType type = this.ParseType(tokens, i, out i);
				fields.Add(new Tuple<string, TypeId>( fName, type.Id));
				if (i + 1 < tokens.Count && tokens[i].Value == ",") // Check if the definition ends, move on to the next token
																	  // and check that it is ','.
				{
					// Move on to the next token, which should be the name of the next token.
					continue; // Move on to the next field.
				}
				else break;
			}
			return fields.ToArray();
		}

		/// <summary>
		/// Make a struct.
		/// </summary>
		/// <param name="name"> The name of the struct to make.</param>
		/// <param name="tokens"> The tokens defining the struct.</param>
		private void MakeRecord(string name, IReadOnlyList<Token> tokens)
		{
			Tuple<string, TypeId>[] fields = null;
			try
			{
				fields = ParseFields(tokens);
            }
			catch (LsnrException e)
			{
				Logging.Log("record", name, e);
				Valid = false;
			}
			catch (Exception e)
			{
				Logging.Log("record", name, e, Path);
				Valid = false;
			}
			if (fields == null) return;
			var recordType = new RecordType(name, fields);
            RecordTypes.Add(name, recordType);
			IncludedTypes.Add(recordType);
		}

		/// <summary>
		/// Make a record.
		/// </summary>
		/// <param name="name"> The name of the record.</param>
		/// <param name="tokens"> The tokens defining the record.</param>
		private void MakeStruct(string name, IReadOnlyList<Token> tokens)
		{
			Tuple<string, TypeId>[] fields = null;
			try
			{
				fields = ParseFields(tokens);
			}
			catch (LsnrException e)
			{
				Logging.Log("struct", name, e);
				Valid = false;
			}
			catch (Exception e)
			{
				Logging.Log("struct", name, e, Path);
				Valid = false;
			}
			if (fields == null) return;
			var structType = new StructType(name, fields);
			StructTypes.Add(name, structType);
			IncludedTypes.Add(structType);
		}

		public LsnResourceThing GetResource()
		{
			return new LsnResourceThing(LoadedTypes.Select(t => t.Id)
				.Union(StructTypes.Select(t => t.Value.Id))
				.Union(RecordTypes.Select(t => t.Value.Id))
				.Union(HostInterfaces.Select(t => t.Value.Id))
				.Union(ScriptObjects.Select(t=>t.Value.Id))
				.ToArray())
			{
				Functions = IncludedFunctions,
				Includes = Includes,
				StructTypes = StructTypes,
				RecordTypes = RecordTypes,
				Usings = Usings,
				HostInterfaces = MyHostInterfaces,
				ScriptObjectTypes = ScriptObjects
				//TODO: Add IncludedTypes.
			};
		}

		public override bool TypeExists(string name) => PreScriptClasses.ContainsKey(name) || PreHostInterfaces.ContainsKey(name) || base.TypeExists(name);

		public override TypeId GetTypeId(string name)
		{
			if (PreScriptClasses.ContainsKey(name))
				return PreScriptClasses[name].Id;
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
			=> base.UniqueScriptObjectTypeExists(name) || PreScriptClasses.Any(p => p.Key == name && p.Value.IsUnique);
	}
}
