using LsnCore;
using Tokens;
using Tokens.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LsnCore.Types;

namespace LSNr
{
	public class PreScript : BasePreScript
	{
		internal const string STRN = "Στρ";
		internal const string SUBN = "SUB";


		private List<Component> Components;



		private Dictionary<string, string> Subs = new Dictionary<string, string>();
		private Dictionary<string, string> Strings = new Dictionary<string, string>();
		private Dictionary<Identifier, List<IToken>> InlineLiterals = new Dictionary<Identifier, List<IToken>>();

		private IScope _CurrentScope = new Scope();

		public override IScope CurrentScope { get { return _CurrentScope; } set { _CurrentScope = value; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="src"></param>
		public PreScript(string src) :base(src)
		{
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
		public LsnScript GetScript()
		{
			return new LsnScript(Components);
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
		public void Parse()
		{
			var parser = new Parser(Tokens, this);
			parser.Parse();
			Components = Parser.Consolidate(parser.Components);
			//CurrentScope.Pop(Components);
		}
		
	}
}
