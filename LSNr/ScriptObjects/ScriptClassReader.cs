using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ReaderRules;

namespace LSNr.ScriptObjects
{
	public interface IPreScriptClass : ITypeContainer
	{
		string Path { get; }

		SymbolType CheckSymbol(string symbol);

		void RegisterField(string name, TypeId id, bool mutable);
		void RegisterAbstractMethod(string name, TypeId returnType, IReadOnlyList<Parameter> parameters);
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
			var mutable = false;
			var indexer = new Indexer<Token>(0, tokens);
			if (indexer.Current.Value == "mut")
			{
				mutable = true;
				indexer.MoveForward();
			}
			if (indexer.Current.Type != TokenType.Identifier)
				throw new LsnrParsingException(indexer.Current, "Invalid field name.", ScriptClass.Path);
			// ToDo: Check that the name is unused.

			var name = indexer.Current.Value;
			if (!indexer.MoveForward()) throw new LsnrParsingException(tokens[0], "Invalid field...", ScriptClass.Path);
			var type = ScriptClass.ParseTypeId(tokens, indexer.LengthBehind, out int i);
			if (i < 0)
				throw new LsnrParsingException(tokens[0], "", ScriptClass.Path);

			ScriptClass.RegisterField(name, type, mutable);
		}
	}

	public sealed class ScriptClassAbstractMethodRule : ScriptClaseStatementRule
	{
		public ScriptClassAbstractMethodRule(IPreScriptClass p) : base(p) { }

		public override bool Check(ISlice<Token> tokens) => tokens[0].Value == "abstract";

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			throw new NotImplementedException();
		}
	}
}
