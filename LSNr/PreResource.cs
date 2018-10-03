using LsnCore;
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
		// Relative to '\src'
		public readonly string RelativePath;

		override public IScope CurrentScope { get; set; } = new VariableTable(new List<Variable>());

		//private readonly Dictionary<Identifier, List<IToken>> InlineLiterals = new Dictionary<Identifier, List<IToken>>();

		private readonly Dictionary<string, RecordType>			MyRecordTypes		= new Dictionary<string, RecordType>();
		private readonly Dictionary<string, StructType>			MyStructTypes		= new Dictionary<string, StructType>();
		private readonly Dictionary<string, LsnFunction>		MyFunctions			= new Dictionary<string, LsnFunction>();
		private readonly Dictionary<string, ScriptClass>		MyScriptClasses		= new Dictionary<string, ScriptClass>();
		private readonly Dictionary<string, List<Token>>		FunctionBodies		= new Dictionary<string, List<Token>>();
		private readonly Dictionary<string, PreHostInterface>	PreHostInterfaces	= new Dictionary<string, PreHostInterface>();
		private readonly Dictionary<string, PreScriptClass>		PreScriptClasses	= new Dictionary<string, PreScriptClass>();
		private readonly Dictionary<string, Token[]>			PreGameValues		= new Dictionary<string, Token[]>();
		private readonly Dictionary<TypeId, Token[]>			PreRecords			= new Dictionary<TypeId, Token[]>();
		private readonly Dictionary<TypeId, Token[]>			PreStructs			= new Dictionary<TypeId, Token[]>();
		private readonly HashSet<TypeId>						UsedGenerics		= new HashSet<TypeId>();

		public PreResource(string src, string path) : base(src,path)
		{
			RelativePath = new string(path.Skip(4).ToArray());
		}

		/// <summary>
		/// Reifies the source...
		/// </summary>
		public void Reify()
		{
			try
			{
				ProcessDirectives();
			}
			catch (Exception)
			{
				//TODO: Logging
				throw;
			}

			try
			{
				Tokenize();
			}
			catch (Exception)
			{
				//TODO: Logging
				throw;
			}

			PreParseGameValues(PreParseFunctions(PreParseTypes()));
			ParseRecordsAndStructs();
			ParseHostInterfaces();
			ParseGameValues();
			PreParseScriptClasses();

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
				string metadata = null;
				var val = Tokens[i].Value;
				// ToDo: Check if it's a keyword.
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
					PreRecords.Add(new TypeId(name), tokens.ToArray());
					//MakeRecord(name, tokens);
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
					PreStructs.Add(new TypeId(name), tokens.ToArray());
					//MakeStruct(name, tokens);
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

					if (Tokens[i].Value == "[")
					{
						i++;
						if (Tokens[i].Type != TokenType.String)
							throw LsnrParsingException.UnexpectedToken(Tokens[i], "a string literal", Path);
						metadata = Tokens[i].Value;
						i++;
						if (Tokens[i].Value != "]")
							throw LsnrParsingException.UnexpectedToken(Tokens[i], "]", Path);
					}
					if (Tokens[i].Value == "<")
					{
						i++; // 'i' points to host name.
						hostName = Tokens[i].Value;
						i++;
						if (Tokens[i].Value == "[")
						{
							if (metadata != null)
								throw new LsnrParsingException(Tokens[i], "a script class can only have one metadata value", Path);
							i++;
							if (Tokens[i].Type != TokenType.String)
								throw LsnrParsingException.UnexpectedToken(Tokens[i], "a string literal", Path);
							metadata = Tokens[i].Value;
							i++;
							if (Tokens[i].Value != "]")
								throw LsnrParsingException.UnexpectedToken(Tokens[i], "]", Path);
						}
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
					var sc = new PreScriptClass(name, this, hostName, unique, metadata, tokens);
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
		private IReadOnlyList<Token> PreParseFunctions(IReadOnlyList<Token> tokens)
		{
			var name = "";
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
						var openCount = 1;
						var closeCount = 0;
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

		private IReadOnlyList<Token> PreParseGameValues(IReadOnlyList<Token> tokens)
		{
			var otherTokens = new List<Token>();
			for (int i = 0; i < tokens.Count; i++)
			{
				if (tokens[i].Type != TokenType.Keyword)
				{
					otherTokens.Add(tokens[i]);
					continue;
				}
				var val = tokens[i].Value;
				if ((val == "game" && (i + 1 < tokens.Count && string.Equals(tokens[++i].Value, "value", StringComparison.OrdinalIgnoreCase)))
					|| val == "gamevalue")
				{
					if (i + 4 >= tokens.Count)
						throw new LsnrParsingException(tokens.Last(), "Unexpected end to game value declaration", Path);
					i++;
					if (tokens[i].Type != TokenType.GameValue)
						throw new LsnrParsingException(tokens[i], $"Improperly formated game value name '{tokens[i].Value}'. A game value name must start with '$'.", Path);
					var name = tokens[i].Value;
					var typeTokens = new List<Token>();
					i++;
					if (tokens[i].Value != ":")
						throw new LsnrParsingException(tokens[i], $"Improperly formated declaration of game value '{name}'. Expected ':', recieved '{tokens[i].Value}'.", Path);
					i++;
					while (tokens[i].Value != ";")
					{
						typeTokens.Add(tokens[i]);
						if (++i >= tokens.Count)
							throw new LsnrParsingException(tokens.Last(), $"Unexpected end to declaration of game value '{name}'.", Path);
					}
					PreGameValues.Add(name, typeTokens.ToArray());
				}
				else otherTokens.Add(tokens[i]);
			}
			return otherTokens;
		}

		private void ParseFunctions()
		{
			foreach(var pair in MyFunctions)
			{
				//try
				//{
					var preFn = new PreFunction(this);
					foreach (var param in pair.Value.Parameters)
						preFn.CurrentScope.CreateVariable(param);
					var parser = new Parser(FunctionBodies[pair.Key], preFn);
					parser.Parse();
					preFn.CurrentScope.Pop(parser.Components);
					if (preFn.Valid)
					{
						var cmps = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
						pair.Value.Code = new ComponentFlattener().Flatten(cmps);
						pair.Value.StackSize = (preFn.CurrentScope as VariableTable)?.MaxSize ?? -1;
					}
					else
						Valid = false;
				/*}
				catch (LsnrException e)
				{
					Logging.Log("function", pair.Key, e);
					Valid = false;
				}
				catch (Exception e)
				{
					Logging.Log("function", pair.Key, e, Path);
					Valid = false;
				}*/
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

		private void PreParseScriptClasses()
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
					MyScriptClasses.Add(sc.Name, sc);
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
				var name = tokens[i].Value;
				if (tokens[++i].Value != ":")
					throw new LsnrParsingException(tokens[i], $"Expected token ':' after parameter name {name} received token '{tokens[i].Value}'.", Path);
				var type = this.ParseTypeId(tokens, ++i, out i);
				var defaultValue = LsnValue.Nil;
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

		private void ParseRecordsAndStructs()
		{
			var records = new List<RecordType>();
			foreach (var pair in PreRecords)
				records.Add(MakeRecord(pair.Key, pair.Value));

			var structs = new List<StructType>();
			foreach (var pair in PreStructs)
				structs.Add(MakeStruct(pair.Key, pair.Value));

			bool CheckRecursion_Base(IHasFieldsType type/*, ref List<Field> fieldTrace*/)
			{
				var value = false;
				foreach (var field in type.FieldsB)
				{
					if (field.Type.Type == type)
						return true;
					var s = field.Type.Type as IHasFieldsType;

					if (s != null)
					{
						if (CheckRecursion(s, type.Id))
							return true;
					}
				}
				return value;
			}

			bool CheckRecursion(IHasFieldsType type, TypeId topType/*, ref List<Field> fieldTrace*/)
			{
				var value = false;
				foreach (var field in type.FieldsB)
				{
					if (field.Type.Type == type)
						return true;
					if (topType == field.Type)
						return true;
					var s = field.Type.Type as IHasFieldsType;

					if (s != null)
					{
						if (CheckRecursion(s, topType))
							return true;
					}
				}
				return value;
			}

			foreach (var structType in structs)
			{
				if (CheckRecursion_Base(structType))
					throw new LsnrParsingException(PreStructs[structType.Id][0], $"The struct type '{structType.Name}' is recursivly defined", Path);
			}
			foreach (var recordType in records)
			{
				if (CheckRecursion_Base(recordType))
					throw new LsnrParsingException(PreStructs[recordType.Id][0], $"The record type '{recordType.Name}' is recursivly defined", Path);
			}
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		private Tuple<string, TypeId>[] ParseFields(Token[] tokens)
		{
			if (tokens.Length < 3) // struct Circle { Radius : double}
			{
				throw new LsnrParsingException(tokens[0], "too few tokens.", Path);
			}
			var fields = new List<Tuple<string, TypeId>>();
			for (int i = 0; i < tokens.Length; i++)
			{
				var fName = tokens[i++].Value; // Get the name of the field, move on to the next token.
				if (i >= tokens.Length) // Make sure the definition does not end..
					throw new LsnrParsingException(tokens[i - 1], "unexpected end of declaration, expected ':'.", Path);
				if (tokens[i++].Value != ":") // Make sure the next token is ':', move on to the next token.
					throw LsnrParsingException.UnexpectedToken(tokens[i - 1], ":", Path);
				if (i >= tokens.Length) // Make sure the definition does not end.
					throw new LsnrParsingException(tokens[i - 1], "unexpected end of declaration, expected type.", Path);
				var tId = this.ParseTypeId(tokens, i, out i);
				fields.Add(new Tuple<string, TypeId>( fName, tId));
				if (i + 1 < tokens.Length && tokens[i].Value == ",") // Check if the definition ends, move on to the next token
																	// and check that it is ','.
				{
					// Move on to the next token, which should be the name of the next field.
					continue; // Move on to the next field.
				}
				break;
			}
			return fields.ToArray();
		}

		/// <summary>
		/// Make a record.
		/// </summary>
		/// <param name="typeId"></param>
		/// <param name="tokens"></param>
		private RecordType MakeRecord(TypeId typeId, Token[] tokens)
		{
			Tuple<string, TypeId>[] fields = null;
			try
			{
				fields = ParseFields(tokens);
			}
			catch (LsnrException e)
			{
				Logging.Log("record", typeId.Name, e);
				Valid = false;
			}
			catch (Exception e)
			{
				Logging.Log("record", typeId.Name, e, Path);
				Valid = false;
			}
			if (fields == null) return null;
			var recordType = new RecordType(typeId, fields);
			MyRecordTypes.Add(typeId.Name, recordType);
			return recordType;
		}

		/// <summary>
		/// Make a struct.
		/// </summary>
		/// <param name="typeId"></param>
		/// <param name="tokens"> The tokens defining the struct.</param>
		private StructType MakeStruct(TypeId typeId, Token[] tokens)
		{
			Tuple<string, TypeId>[] fields = null;
			try
			{
				fields = ParseFields(tokens);
			}
			catch (LsnrException e)
			{
				Logging.Log("struct", typeId.Name, e);
				Valid = false;
			}
			catch (Exception e)
			{
				Logging.Log("struct", typeId.Name, e, Path);
				Valid = false;
			}
			if (fields == null) return null;
			var structType = new StructType(typeId, fields);
			MyStructTypes.Add(typeId.Name, structType);
			return structType;
		}

		private void ParseGameValues()
		{
			foreach (var pair in PreGameValues)
			{
				var type = this.ParseTypeId(pair.Value, 0, out int i);
			}
		}

		private TypeId[] GetTypeIds()
		{
			return new List<TypeId> {new TypeId("void") }
				.Union(LoadedTypes		.Select(t => t.Id))
				.Union(MyStructTypes	.Select(t => t.Value.Id))
				.Union(MyRecordTypes	.Select(t => t.Value.Id))
				.Union(MyHostInterfaces	.Select(t => t.Value.Id))
				.Union(MyScriptClasses	.Select(t => t.Value.Id))
				.Union(UsedGenerics)
				.Distinct()
				.ToArray();
		}

		public LsnResourceThing GetResource()
		{
			return new LsnResourceThing(GetTypeIds())
			{
				Functions = MyFunctions.ToDictionary(p=> p.Key, p => (Function)p.Value),
				Includes = new List<string>(),
				StructTypes = MyStructTypes,
				RecordTypes = MyRecordTypes,
				Usings = Usings,
				HostInterfaces = MyHostInterfaces,
				ScriptClassTypes = MyScriptClasses
			};
		}

		public override bool TypeExists(string name) => PreScriptClasses.ContainsKey(name) || PreHostInterfaces.ContainsKey(name)
			|| PreStructs.Keys.Any(k => k.Name == name) || PreRecords.Keys.Any(k => k.Name == name) || base.TypeExists(name);

		public override TypeId GetTypeId(string name)
		{
			if (PreScriptClasses.ContainsKey(name))
				return PreScriptClasses[name].Id;
			if (PreHostInterfaces.ContainsKey(name))
				return PreHostInterfaces[name].HostInterfaceId;
			if (PreRecords.Keys.Any(k => k.Name == name))
				return PreRecords.Keys.First(k => k.Name == name);
			if (PreStructs.Keys.Any(k => k.Name == name))
				return PreStructs.Keys.First(k => k.Name == name);
			return base.GetTypeId(name);
		}

		public override SymbolType CheckSymbol(string name)
		{
			if (MyFunctions.ContainsKey(name) || LoadedExternallyDefinedFunctions.ContainsKey(name))
				return SymbolType.Function;
			if (CurrentScope.VariableExists(name))
				return SymbolType.Variable;
			if (UniqueScriptObjectTypeExists(name))
				return SymbolType.UniqueScriptObject;
			if (TypeExists(name))
				return SymbolType.Type;

			return SymbolType.Undefined;
		}

		public override bool UniqueScriptObjectTypeExists(string name)
			=> base.UniqueScriptObjectTypeExists(name) || PreScriptClasses.Any(p => p.Key == name && p.Value.IsUnique);

		public override void GenericTypeUsed(TypeId typeId)
		{
			if (!UsedGenerics.Contains(typeId))
				UsedGenerics.Add(typeId);
		}

		public override LsnType GetType(string name)
		{
			if (name.Contains('`'))
			{
				var names = name.Split('`');
				if (GenericTypeExists(names[0]))
				{
					var generic = GetGenericType(names[0]);
					return generic.GetType(names.Skip(1).Select(n => GetType(n)).Select(t => t.Id).ToArray());
				}

				throw new LsnrTypeNotFoundException(Path, name);
			}
			var type = LoadedTypes.FirstOrDefault(t => t.Name == name);
			if (type != null) return type;

			if (MyScriptClasses.ContainsKey(name))
				return MyScriptClasses[name];
			if (MyHostInterfaces.ContainsKey(name))
				return MyHostInterfaces[name];
			if (MyStructTypes.ContainsKey(name))
				return MyStructTypes[name];
			if (MyRecordTypes.ContainsKey(name))
				return MyRecordTypes[name];
			if (PreHostInterfaces.ContainsKey(name))
				return PreHostInterfaces[name].HostInterfaceId.Type;
			if (PreScriptClasses.ContainsKey(name))
				return PreScriptClasses[name].Id.Type;

			if (type == null)
				throw new LsnrTypeNotFoundException(Path, name);
			return type;
		}

		public override Function GetFunction(string name)
			=> MyFunctions.ContainsKey(name) ? MyFunctions[name] : base.GetFunction(name);
	}
}
