using LSN_Core;
using LSN_Core.Compile;
using LSN_Core.Compile.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class PreScript
	{
		internal const string STRN = "Στρ";
		internal const string SUBN = "SUB";


		private List<Component> Components;

		/// <summary>
		/// 
		/// </summary>
		public bool Mutable = false;

		/// <summary>
		/// 
		/// </summary>
		private readonly string Source;


		public string Text;

		private Dictionary<string, string> Subs = new Dictionary<string, string>();
		private Dictionary<string, string> Strings = new Dictionary<string, string>();
		private Dictionary<Identifier, IToken> InlineLiterals;

		/// <summary>
		/// The current scope.
		/// </summary>
		public Scope CurrentScope { get; set; }

		/// <summary>
		/// Is this script valid (free of errors)?
		/// </summary>
		public bool Valid { get; set; }

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

		protected string ProcessDirectives(string source)
		{
			if (source.Contains("#mut"))
			{
				Mutable = true;
				source = source.Replace("#mut", "");
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


		public void Parse()
		{
			var parser = new Parser(Tokens, this);
			parser.Parse();
			Components = Parser.Consolidate(parser.Components);
		}

	}
}
