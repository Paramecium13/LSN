using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Utilities;
using LSNr.ReaderRules;

namespace LSNr.ScriptObjects
{
	public interface IPreScriptClass : ITypeContainer
	{

	}

	public sealed class ScriptClassReader : RuledReader<ScriptClaseStatementRule, ScriptClassBodyRule>
	{
		protected override IEnumerable<ScriptClaseStatementRule> StatementRules { get; } = new ScriptClaseStatementRule[] { };

		protected override IEnumerable<ScriptClassBodyRule> BodyRules { get; } = new ScriptClassBodyRule[] { };

		public ScriptClassReader(ISlice<Token> tokens) : base(tokens) { }

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes){}
	}

	// All types will have been registered before these rules are applied.

	public abstract class ScriptClaseStatementRule : IReaderStatementRule
	{
		protected readonly IPreScriptClass ScriptClass;
		protected ScriptClaseStatementRule(IPreScriptClass scriptClass) { ScriptClass = scriptClass; }

		public abstract void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> tokens);
	}

	public abstract class ScriptClassBodyRule : IReaderBodyRule
	{
		protected readonly IPreScriptClass ScriptClass;
		protected ScriptClassBodyRule(IPreScriptClass scriptClass) { ScriptClass = scriptClass; }

		public abstract void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> head);
	}

	// NOTE: Put this last in the rule list.
	public sealed class ScriptClassFieldRule : ScriptClaseStatementRule
	{
		public ScriptClassFieldRule(IPreScriptClass p) : base(p) { }

		public override bool Check(ISlice<Token> tokens)
		{
			if (tokens[0].Value == "mut") return true;
			return tokens.Any(t => t.Value == ":");
		}

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			throw new NotImplementedException();
		}
	}
}
