using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Utilities;
using LSNr.ReaderRules;

namespace LSNr.ScriptObjects
{
	public interface IPreScriptClass
	{

	}

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

	public sealed class ScriptClassReader : RuledReader<ScriptClaseStatementRule, ScriptClassBodyRule>
	{
		protected override IEnumerable<ScriptClaseStatementRule> StatementRules { get; } = new ScriptClaseStatementRule[] { };

		protected override IEnumerable<ScriptClassBodyRule> BodyRules { get; } = new ScriptClassBodyRule[] { };

		public ScriptClassReader(ISlice<Token> tokens) : base(tokens) { }

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes){}
	}
}
