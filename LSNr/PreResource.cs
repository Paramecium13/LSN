using LsnCore;
using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tokens;
using Tokens.Tokens;

namespace LSNr
{
	public class PreResource : IPreScript, ITypeContainer
	{
		internal const string STRN = "Στρ";
		internal const string SUBN = "SUB";

		public Scope CurrentScope { get; set; }

		private bool _Mutable = false;

		/// <summary>
		/// Are variables defined in this script mutable?
		/// </summary>
		public bool Mutable { get { return _Mutable; } private set { _Mutable = value; } }

		/// <summary>
		/// Is this script valid (free of errors)?
		/// </summary>
		public bool Valid { get; set; } = true;

		private readonly string Source;
		public string Text;
		private List<IToken> Tokens;

		private readonly Dictionary<string, string> Subs = new Dictionary<string, string>();
		private readonly Dictionary<string, string> Strings = new Dictionary<string, string>();
		private readonly Dictionary<Identifier, List<IToken>> InlineLiterals = new Dictionary<Identifier, List<IToken>>();
		private readonly Dictionary<string, LsnStructType> StructTypes = new Dictionary<string, LsnStructType>();
		private readonly Dictionary<string, RecordType> RecordTypes = new Dictionary<string, RecordType>();
		
		private readonly Dictionary<string, Function> Functions = new Dictionary<string, Function>();
		private readonly Dictionary<string, List<IToken>> FunctionBodies = new Dictionary<string, List<IToken>>();
		private readonly List<LsnType> Types = LsnType.GetBaseTypes();
		private readonly List<GenericType> GenericTypes = LsnType.GetBaseGenerics();

		/*private class PreFunction
		{
			public readonly string Name;
			public readonly List<Parameter> Parameters;
			public readonly List<IToken> Tokens;

			public PreFunction(string name, List<Parameter> parameters, List<IToken> tokens)
			{
				Name = name; Parameters = parameters; Tokens = tokens;
			}



		}*/

		/// <summary>
		/// Reifies the source...
		/// </summary>
		public void Reify()
		{
			ProcessDirectives();
			Tokenize();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool FunctionExists(string name)
			=> Functions.ContainsKey(name) || false /*Includes.Any(i => i.Functions.ContainsKey(name))*/;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool FunctionIsIncluded(string name)
			=> Functions.ContainsKey(name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Function GetFunction(string name)
		{
			if (FunctionIsIncluded(name))
				return Functions[name];
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool TypeExists(string name)
			=> Types.Any(t => t.IsName(name));

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public LsnType GetType(string name)
			=> Types.Where(t => t.IsName(name)).FirstOrDefault();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool GenericTypeExists(string name)
			=> GenericTypes.Any(g => g.Name == name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public GenericType GetGenericType(string name)
			=> GenericTypes.Where(t => t.Name == name).FirstOrDefault();

		/// <summary>
		/// 
		/// </summary>
		public void ProcessDirectives() { Text = ProcessDirectives(Text); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		protected string ProcessDirectives(string source)
		{
			if (Regex.IsMatch(source, "#include", RegexOptions.IgnoreCase))
			{
				//Process #include statements
			}
			if (source.Contains("#mut"))
			{
				Mutable = true;
				source = source.Replace("#mut", "");
			}
			if (source.Contains("#no_std"))
			{
				source = source.Replace("#no_std", "");
			}
			else
			{
				// Load the standard library.
			}
			source = Tokenizer.ReplaceAndStore(source, @"(?s)#sub(.(?<!#endsub))*#endsub", SUBN, Subs);
			var x = new string[Subs.Count];
			Subs.Keys.CopyTo(x, 0);
			foreach (string key in x)
			{
				Subs[key] = Subs[key].Replace("#sub", "").Replace("#endsub", "");
			}
			return source;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Tokenize() { Tokens = Tokenizer.Tokenize(Text); }

		/// <summary>
		/// Go through the source, parsing structs and records.
		/// </summary>
		/// <returns> Tokens that are not part of a struct or record.</returns>
		private List<IToken> ParsestructsAndRecords()
		{
			var otherTokens = new List<IToken>();
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
					var tokens = new List<IToken>();
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
					var tokens = new List<IToken>();
					while (Tokens[i].Value != "}") tokens.Add(Tokens[i++]);
					MakeRecord(name, tokens);
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
		private List<IToken> ParseFunctions(List<IToken> tokens)
		{
			var otherTokens = new List<IToken>();
			for (int i = 0; i < tokens.Count; i++)
			{
				if (tokens[i].Value == "fn")
				{
					string name = tokens[++i].Value;
					if(tokens[++i].Value != "(")
					{
						Console.WriteLine($"Error in parsing function {name} expected token '(', recieved '{tokens[i].Value}'.");
						Valid = false; continue;
					}
					var paramTokens = new List<IToken>();
					while (tokens[++i].Value != ")") // This starts with the token after '('.
						paramTokens.Add(tokens[i]);
					List<Parameter> paramaters = null;
					try
					{
						paramaters = ParseParameters(paramTokens);
					}
					catch (Exception e)
					{
						Console.WriteLine($"Error in parsing parameters of function {name}");
						Console.WriteLine("\t" + e.Message);
						Valid = false; continue;
					}
					// At this point, the current token (i.e. tokens[i].Value) is ')'.
					LsnType returnType = null;
					if (tokens[++i].Value == "->")
					{
						if(tokens[++i].Value == "(")
						{ // The current token is the token after '->'.
							if(tokens[++i].Value != ")")
							{
								Console.WriteLine($"Error in parsing function {name} expected token '(', recieved '{tokens[i].Value}'.");
								Valid = false; continue;
							}
						}
						else
						{ // The current token is the token after '->'.
							try
							{
								returnType = this.ParseType(tokens, i, out i);
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
						Console.WriteLine($"Error in parsing function {name} expected token '{{', recieved '{tokens[i].Value}'.");
						Valid = false; continue;
					}
					var fnBody = new List<IToken>();
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
					Functions.Add(name, new LSN_Function(paramaters, returnType, name));
					FunctionBodies.Add(name, fnBody);
				}
				else otherTokens.Add(tokens[i]);
				
			}
			return otherTokens;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		private List<Parameter> ParseParameters(List<IToken> tokens)
		{
			var paramaters = new List<Parameter>();
			ushort index = 0;
			for(int i = 0; i < tokens.Count; i++)
			{
				string name = tokens[i].Value;
				if(tokens[++i].Value != ":")
					throw new ApplicationException($"Error: Expected token ':' after parameter name {name} recieved token '{tokens[i].Value}'.");
                LsnType type = this.ParseType(tokens, ++i, out i);
				ILsnValue defaultValue = null;
				if (tokens[i].Value == "=")
				{
					if (tokens[++i] is StringToken)
					{
						if (type != LsnType.string_)
							throw new ApplicationException($"Error in parsing parameter {name}: cannot assign a default value of type string to a parameter of type {type.Name}");
						defaultValue = new StringValue(tokens[i].Value);
						if (i + 1 < tokens.Count) i++;
					}
					else if (tokens[i] is IntToken)
					{
						if (type != LsnType.int_)
						{
							if (type == LsnType.double_)
							{
								defaultValue = new DoubleValue((tokens[i] as IntToken?)?.IVal ?? 0);
							}
							else
								throw new ApplicationException($"Error in parsing parameter {name}: cannot assign a default value of type int to a parameter of type {type.Name}");
						}
						else defaultValue = new IntValue((tokens[i] as IntToken?)?.IVal ?? 0);
						if (i + 1 < tokens.Count) i++;
					}
					else if (tokens[i] is FloatToken)
					{
						if (type != LsnType.double_)
							throw new ApplicationException($"Error in parsing parameter {name}: cannot assign a default value of type double to a parameter of type {type.Name}");
						defaultValue = new DoubleValue((tokens[i] as FloatToken?)?.DVal ?? 0.0);
						if(i + 1 < tokens.Count) i++;
					}
					// Bools and other stuff...
					else throw new ApplicationException($"Error in parsing default value for parameter {name}.");
				}
				paramaters.Add(new Parameter(name, type, defaultValue, index++));
				if (tokens[i].Value != ",")
					throw new ApplicationException($"Error: expected token ',' after definition of parameter {name}, recieved '{tokens[i].Value}'.");
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
		private Dictionary<string, LsnType> ParseFields(string name, string typeOfType, List<IToken> tokens)
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
					Console.WriteLine($"Error in {typeOfType} {name}: unexpected token {tokens[i - 1]}, expected \':\'");
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
				if (i + 1 < tokens.Count && tokens[++i].Value == ",") // Check if the definition ends, move on to the next token
																	  // and check that it is ','.
				{
					++i; // Move on to the next token, which should be the name of the next token.
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
		private void MakeStruct(string name, List<IToken> tokens)
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
		}

		/// <summary>
		/// Make a record.
		/// </summary>
		/// <param name="name"> The name of the record.</param>
		/// <param name="tokens"> The tokens defining the record.</param>
		private void MakeRecord(string name, List<IToken> tokens)
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
		}


		public LsnResourceThing GetResource()
		{
			throw new NotImplementedException();
		}
	}
}
