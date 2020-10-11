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
	public interface IPreState : IBasePreScriptClass
	{
		ScriptClassMethod RegisterMethod(string name, TypeId returnType, IReadOnlyList<Parameter> parameters);
		EventListener RegisterEventListener(string name, IReadOnlyList<Parameter> parameters);

		event Action<IPreState> ParsingProcBodies;
		event Action<IPreState> ParsingSignaturesB;
	}

	public sealed class StateReader : RuledReader<IReaderStatementRule, StateBodyRule>
	{
		protected override IEnumerable<IReaderStatementRule> StatementRules { get; }
		protected override IEnumerable<StateBodyRule> BodyRules { get; }

		public StateReader(ISlice<Token> tokens, IPreState state) : base(tokens)
		{
			StatementRules = new IReaderStatementRule[0];
			BodyRules = new StateBodyRule[]
			{
				new StateEventListenerRule(state),
				new StateMethodRule(state)
			};
		}

		public void Read()
		{
			ReadTokens();
		}

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes){}
	}

	public abstract class StateBodyRule : IReaderBodyRule
	{
		protected readonly IPreState State;
		protected StateBodyRule(IPreState state) { State = state; }

		public abstract void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> head);
		protected string GetName(Indexer<Token> i, string memberTypeName)
		{
			if (i.Current.Type != TokenType.Identifier)
				throw new LsnrParsingException(i.Current, $"'{i.Current.Value}' is not a valid {memberTypeName} name.", State.Path);
			var s = State.CheckSymbol(i.Current.Value);
			if (s != SymbolType.Undefined && s != SymbolType.ScriptClassMethod)
				throw new LsnrParsingException(i.Current, $"Cannot name a {memberTypeName} '{i.Current.Value}'. There is already a(n) {s.ToString()} with that name.", State.Path);
			return i.Current.Value;
		}

		protected IReadOnlyList<Parameter> ParseParameters(Indexer<Token> i, string memberTypeName, string memberName)
		{
			if (!i.MoveForward() || i.Current.Value != "(" || !i.MoveForward())
				throw new LsnrParsingException(i.Current, $"Error parsing {memberTypeName} {memberName}: No parameter list defined", State.Path);
			var argTokens = i.SliceWhile(t => t.Value != ")", out bool err);
			if (err)
				throw new LsnrParsingException(i.Current, $"Error parsing {memberTypeName} {memberName}: No parameter list defined", State.Path);
			return State.ParseParameters(argTokens, 0);
		}

		protected TypeId ParseReturnType(Indexer<Token> index, string memberTypeName, string memberName)
		{
			TypeId ret = null;
			if (!index.MoveForward() || index.Current.Value == "{") return null;
			if (index.Current.Value != "->" || !index.MoveForward())
				throw new LsnrParsingException(index.Current, $"Error parsing {memberTypeName} {memberName}: Expected '->' or end of definition; received '{index.Current.Value}'.",
					State.Path);
			if (index.Current.Value == "(")
			{
				if (!index.MoveForward() || index.Current.Value != ")")
					throw new LsnrParsingException(index.Current, "...", State.Path);
			}
			else
			{
				var tTokens = index.SliceWhile(t => t.Value != ";" && t.Value != "{", out bool err);
				ret = State.ParseTypeId(tTokens, 0, out int x);
				if (ret == null)
					throw new LsnrParsingException(index.Current, "Type not found...", State.Path);
			}
			return ret;
		}
	}

	public sealed class StateEventListenerRule : StateBodyRule
	{
		public StateEventListenerRule(IPreState s) : base(s) { }

		public override bool Check(ISlice<Token> head) => head[0].Value == "on";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var index = new Indexer<Token>(1, head);
			var name = GetName(index, "event listener");
			var args = ParseParameters(index, "event listener", name);
			if (index.MoveForward() && index.Current.Value != "{")
				throw new LsnrParsingException(index.Current, "", State.Path);
			var listener = State.RegisterEventListener(name, args);
			var comp = new ScriptClassEventListenerComponent(listener, body, head[0]);
			State.ParsingProcBodies += comp.OnParsingProcBodies;
			State.ParsingSignaturesB += comp.Validate;
		}
	}

	public sealed class StateMethodRule : StateBodyRule
	{
		public StateMethodRule(IPreState s) : base(s) { }

		public override bool Check(ISlice<Token> head) => head[0].Value == "fn";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var index = new Indexer<Token>(1, head);
			var name = GetName(index, "method");
			var args = ParseParameters(index, "method", name);
			var ret = ParseReturnType(index, "method", name);
			var method = State.RegisterMethod(name, ret, args);
			var comp = new ScriptClassMethodComponent(method, body);
			State.ParsingProcBodies += comp.OnParsingProcBodies;
		}
	}
}
