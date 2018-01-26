using LsnCore;
using LSNr.Optimization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Statements;

namespace LSNr
{
	public class PreScript : BasePreScript
	{
		private List<Component> Components;

		private Statement[] Statements;

		//private Dictionary<string, string> Subs = new Dictionary<string, string>();
		//private Dictionary<Identifier, List<IToken>> InlineLiterals = new Dictionary<Identifier, List<IToken>>();

		private IScope _CurrentScope = new Scope();

		public override IScope CurrentScope { get { return _CurrentScope; } set { _CurrentScope = value; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="src"></param>
		public PreScript(string src, string path) :base(src,path)
		{
			Text = Source; // ??
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
			return new LsnScript(Statements);
		}

		/// <summary>
		/// 
		/// </summary>
		public void ProcessDirectives() { Text = ProcessDirectives(Text); }

		/// <summary>
		/// 
		/// </summary>
		public void Parse()
		{
			var parser = new Parser(Tokens, this);
			parser.Parse();
			Components = Parser.Consolidate(parser.Components);
			//CurrentScope.Pop(Components);
			var flattener = new ComponentFlattener();
			Statements = flattener.Flatten(Components);
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

	}
}
