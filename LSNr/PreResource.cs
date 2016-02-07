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
	public class PreResource : IPreScript
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


		private void ParsestructsAndRecords()
		{
			for(int i = 0; i < Tokens.Count; i++)
			{
				var val = Tokens[i].Value;
				if(val == "struct")
				{
					string name = Tokens[++i].Value;
					if (Tokens[++i].Value != "{")
					{
						Console.WriteLine($"Error in parsing struct {name}: invalid token: {Tokens[i]}, expected {{.");
						Valid = false;
					}
					var tokens = new List<IToken>();
					while (Tokens[i].Value != "}") tokens.Add(Tokens[i++]);
					MakeStruct(name, tokens);
				}
				else if(val == "record")
				{
					string name = Tokens[++i].Value;
					if (Tokens[++i].Value != "{")
					{
						Console.WriteLine($"Error in parsing record {name}: invalid token: {Tokens[i]}, expected {{.");
						Valid = false;
					}
					var tokens = new List<IToken>();
					while (Tokens[i].Value != "}") tokens.Add(Tokens[i++]);
					MakeRecord(name, tokens);
				}
			}
		}


		private void MakeStruct(string name, List<IToken> tokens)
		{
			if (tokens.Count < 3)
			{
				Console.WriteLine($"Error, invalid struct {name}.");
				Valid = false;
				return;
			}
			var fields = new Dictionary<string, LSN_Type>();
			for(int i = 0; i < tokens.Count; i++)
			{
				string fName = tokens[i++].Value;
				if(i >= tokens.Count)
				{
					Valid = false;
					Console.WriteLine($"Error in struct {name}: unexpected end of declaration, expected \':\'.");
					return;
				}
				if(tokens[i++].Value != ":")
				{
					Valid = false;
					Console.WriteLine($"Error in struct {name}: unexpected token {tokens[i - 1]}, expected \':\'");
					return;
				}
				if (i >= tokens.Count)
				{
					Valid = false;
					Console.WriteLine($"Error in struct {name}: unexpected end of declaration, expected type.");
					return;
				}
				if(! TypeExists(tokens[i].Value))
				{
					Valid = false;
					Console.WriteLine($"Error in struct {name}: no type named{tokens[i].Value} could be found.");
					return;
				}
				var type = GetType(tokens[i].Value);
				fields.Add(fName, type);
				if (i + 1 < tokens.Count && tokens[++i].Value == ",") continue; //Move on to the next field
				else break;
			}
			var structType = new LSN_StructType(name, fields);
            StructTypes.Add(name, structType);
		}


		private void MakeRecord(string name, List<IToken> tokens)
		{
			if (tokens.Count < 3)
			{
				Console.WriteLine($"Error, invalid record {name}.");
				Valid = false;
				return;
			}
			var fields = new Dictionary<string, LSN_Type>();
			for (int i = 0; i < tokens.Count; i++)
			{
				string fName = tokens[i++].Value;
				if (i >= tokens.Count)
				{
					Valid = false;
					Console.WriteLine($"Error in record {name}: unexpected end of declaration, expected \':\'.");
					return;
				}
				if (tokens[i++].Value != ":")
				{
					Valid = false;
					Console.WriteLine($"Error in record {name}: unexpected token {tokens[i - 1]}, expected \':\'");
					return;
				}
				if (i >= tokens.Count)
				{
					Valid = false;
					Console.WriteLine($"Error in record {name}: unexpected end of declaration, expected type.");
					return;
				}
				if (!TypeExists(tokens[i].Value))
				{
					Valid = false;
					Console.WriteLine($"Error in record {name}: no type named{tokens[i].Value} could be found.");
					return;
				}
				var type = GetType(tokens[i].Value);
				fields.Add(fName, type);
				if (i + 1 < tokens.Count && tokens[++i].Value == ",") continue; //Move on to the next field
				else break;
			}
			var recordType = new RecordType(name, fields);
			RecordTypes.Add(name, recordType);
		}

		public bool TypeExists(string name)
			=> Types.Any(t => t.IsName(name));

		public LSN_Type GetType(string name)
			=> Types.Where(t => t.IsName(name)).FirstOrDefault();

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

		public LSN_Script GetScript()
		{
			throw new NotImplementedException();
		}
	}
}
