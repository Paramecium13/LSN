using LSN_Core;
using LSN_Core.Compile;
using LSN_Core.Compile.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LSNr
{
	public class PreScript : IPreScript
	{
		internal const string STRN = "Στρ";
		internal const string SUBN = "SUB";


		private List<Component> Components;

		private bool _Mutable = false;

		/// <summary>
		/// Are variables defined in this script mutable?
		/// </summary>
		public bool Mutable {get { return _Mutable; } private set { _Mutable = value; } }

		/// <summary>
		/// 
		/// </summary>
		private readonly string Source;


		public string Text;

		private Dictionary<string, string> Subs = new Dictionary<string, string>();
		private Dictionary<string, string> Strings = new Dictionary<string, string>();
		private Dictionary<Identifier, IToken> InlineLiterals;

		private Dictionary<string, Function> Functions = new Dictionary<string, Function>();

		/// <summary>
		/// The current scope.
		/// </summary>
		public Scope CurrentScope { get; set; } = new Scope();

		/// <summary>
		/// Is this script valid (free of errors)?
		/// </summary>
		public bool Valid { get; set; } = true;

		private List<IToken> Tokens;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="src"></param>
		public PreScript(string src)
		{
			Source = src;
			Text = Source;
		}

		/// <summary>
		/// Reifies the source...
		/// </summary>
		public void Reify()
		{
			ProcessDirectives();
			//TODO: Process strings. 
			Tokenize();
			Parse();
		}

		/// <summary>
		/// Gets the script. Clients must call Reify() and check if this is Valid before calling GetScript().
		/// </summary>
		/// <returns></returns>
		public LSN_Script GetScript()
		{
			return new LSN_Script(Components);
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
			if(Regex.IsMatch(source, "#include",RegexOptions.IgnoreCase))
			{
				//Process #include statements
			}
			if (source.Contains("#mut"))
			{
				Mutable = true;
				source = source.Replace("#mut", "");
			}
			if(source.Contains("#no_std"))
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
		/// 
		/// </summary>
		public void Parse()
		{
			var parser = new Parser(Tokens, this);
			parser.Parse();
			Components = Parser.Consolidate(parser.Components);
			//CurrentScope.Pop(Components);
		}

		/// <summary>
		/// Does a function with the provided name exist.
		/// </summary>
		/// <param name="name"> The name to check.</param>
		/// <returns></returns>
		public bool FunctionExists(string name) => Functions.ContainsKey(name);

		/// <summary>
		/// Get the function with the given name. WARNING: The function may or may not be included in the script.
		/// This can be determined using the 'FuncionIsIncluded(string name)' method. If it is not included, function calls
		/// to it must be constructed using its name, otherwise they will end up including it in this script.
		/// </summary>
		/// <param name="name">The name of the function to get.</param>
		/// <returns></returns>
		public Function GetFunction(string name) => Functions[name];

		/// <summary>
		/// Is the function included (#include) in this script?
		/// </summary>
		/// <param name="name">The name of the function.</param>
		/// <returns></returns>
		public bool FunctionIsIncluded(string name)
		{
			throw new NotImplementedException();
		}
	}
}
