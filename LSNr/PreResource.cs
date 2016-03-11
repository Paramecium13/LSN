using LSN_Core;
using LSN_Core.Types;
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

		private Dictionary<string, string> Subs = new Dictionary<string, string>();
		private Dictionary<string, string> Strings = new Dictionary<string, string>();
		private Dictionary<Identifier, List<IToken>> InlineLiterals = new Dictionary<Identifier, List<IToken>>();
		private Dictionary<string, LSN_StructType> StructTypes = new Dictionary<string, LSN_StructType>();
		private Dictionary<string, RecordType> RecordTypes = new Dictionary<string, RecordType>();

		private Dictionary<string, Function> Functions = new Dictionary<string, Function>();
		private List<LSN_Type> Types = LSN_Type.GetBaseTypes();


		/// <summary>
		/// Reifies the source...
		/// </summary>
		public void Reify()
		{
			ProcessDirectives();
			Tokenize();
        }

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
		public LSN_Type GetType(string name)
			=> Types.Where(t => t.IsName(name)).FirstOrDefault();


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
		private void ParsestructsAndRecords()
		{
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
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="typeOfType">struct or record.</param>
		/// <param name="tokens"></param>
		/// <returns></returns>
		private Dictionary<string, LSN_Type> ParseFields(string name, string typeOfType, List<IToken> tokens)
		{
			if (tokens.Count < 3) // struct Circle { Radius : double}
			{
				Console.WriteLine($"Error, invalid struct {name}.");
				Valid = false;
				return null;
			}
			var fields = new Dictionary<string, LSN_Type>();
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
				LSN_Type type = this.ParseType(tokens, i, out i);
				/*if (TypeExists(tokens[i].Value))
				{
					type = GetType(tokens[i].Value);
				}
				else
				{
					// Temporary stop gap.
					if(tokens[i].Value == "Vector")
					{
						if(tokens[++i].Value != "<")
						{
							Valid = false;
							Console.WriteLine($"Error in {typeOfType} {name}: expected '<', recieved '{tokens[i].Value}'.");
							return null;
						}
						if (TypeExists(tokens[++i].Value))
						{
							type = VectorType.GetVectorType(GetType(tokens[i].Value));
						}
						else
						{
							Valid = false;
							Console.WriteLine($"Error in {typeOfType} {name}: no type named {tokens[i].Value} could be found.");
							return null;
						}
						// Todo: Allow Vectors of Vectors:Make a method to get a type from a list of tokens,
						// use recursion with generics.
					}
					//Todo: Check for generic types, number of generic parameters, etc.
					//if (GenericTypeExists(tokens[i].Value)) 
					//{
					//
					//}
					
					else
					{
						Valid = false;
						Console.WriteLine($"Error in {typeOfType} {name}: no type named {tokens[i].Value} could be found.");
						return null;
					}
				}*/
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
			Dictionary<string, LSN_Type> fields = null;
			try
			{
				fields = ParseFields(name, "struct", tokens);
            }
			catch (Exception e)
			{
				Console.WriteLine($"Error in parsing struct {name}.");
#pragma warning disable 0162
				if (false/*Show exeption info*/)
					Console.WriteLine(e.Message);
#pragma warning restore 0162
			}
			if (fields == null) return;
			var structType = new LSN_StructType(name, fields);
            StructTypes.Add(name, structType);
		}

		/// <summary>
		/// Make a record.
		/// </summary>
		/// <param name="name"> The name of the record.</param>
		/// <param name="tokens"> The tokens defining the record.</param>
		private void MakeRecord(string name, List<IToken> tokens)
		{
			Dictionary<string, LSN_Type> fields = null;
			try
			{
				fields = ParseFields(name, "struct", tokens);
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in parsing struct {name}.");
#pragma warning disable 0162
				if (false/*ShowExeptionInfo*/)
					Console.WriteLine(e.Message);
#pragma warning restore 0162
			}
			if (fields == null) return;
			var recordType = new RecordType(name, fields);
			RecordTypes.Add(name, recordType);
		}


		public LSN_Script GetScript()
		{
			throw new NotImplementedException();
		}
	}
}
