using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.Converations;
using LSNr.ReaderRules;

namespace LSNr.ScriptObjects
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PreScript")]
	public interface IBasePreScriptClass : IFunctionContainer
	{
		TypeId Id { get; }
		TypeId HostId { get; }
		HostInterfaceType Host { get; }

		int GetStateIndex(string name);
		Field GetField(string val);
		bool StateExists(string stateName);
		bool MethodExists(string value);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PreScript")]
	public interface IPreScriptClass : IBasePreScriptClass
	{
		void RegisterField(string name, TypeId id, bool mutable);
		void RegisterAbstractMethod(string name, TypeId returnType, IReadOnlyList<Parameter> parameters);
		ScriptClassMethod RegisterMethod(string name, TypeId returnType, IReadOnlyList<Parameter> parameters, bool isVirtual);
		EventListener RegisterEventListener(string name, IReadOnlyList<Parameter> parameters);
		ScriptClassConstructor RegisterConstructor(IReadOnlyList<Parameter> parameters);

		event Action<IPreScriptClass> ParsingProcBodies;
		event Action<IPreScriptClass> ParsingSignaturesB;
		event Action<IPreScriptClass> ParsingStateSignatures;

		ScriptClassState RegisterState(string name, bool auto, IReadOnlyList<ScriptClassMethod> methods, IReadOnlyList<EventListener> eventListeners);
	}

	public sealed class ScriptClassReader : RuledReader<ScriptClassStatementRule, ScriptClassBodyRule>
	{
		protected override IEnumerable<ScriptClassStatementRule> StatementRules { get; }

		protected override IEnumerable<ScriptClassBodyRule> BodyRules { get; }

		public ScriptClassReader(ISlice<Token> tokens, IPreScriptClass pre) : base(tokens)
		{
			StatementRules = new ScriptClassStatementRule[] {
				new ScriptClassAbstractMethodRule(pre),
				new ScriptClassFieldRule(pre)
			};
			BodyRules = new ScriptClassBodyRule[] {
				new ScriptClassConstuctorRule(pre),
				new ScriptClassEventListenerRule(pre),
				new ScriptClassMethodRule(pre),
				new ScriptClassStateRule(pre),
				new ScriptClassConversationRule(pre)
			};
		}

		public void Read()
		{
			ReadTokens();
		}

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes){}
	}

	// All types will have been registered before these rules are applied.

	public abstract class ScriptClassRule
	{
		protected readonly IPreScriptClass ScriptClass;
		protected ScriptClassRule(IPreScriptClass scriptClass) { ScriptClass = scriptClass; }

		protected string GetName(Indexer<Token> i, string memberTypeName)
		{
			if (i.Current.Type != TokenType.Identifier)
				throw new LsnrParsingException(i.Current, $"'{i.Current.Value}' is not a valid {memberTypeName} name.", ScriptClass.Path);
			var s = ScriptClass.CheckSymbol(i.Current.Value);
			if (s != SymbolType.Undefined)
				throw new LsnrParsingException(i.Current, $"Cannot name a {memberTypeName} '{i.Current.Value}'. There is already a(n) {s.ToString()} with that name.", ScriptClass.Path);
			return i.Current.Value;
		}

		protected IReadOnlyList<Parameter> ParseParameters(Indexer<Token> i, string memberTypeName, string memberName)
		{
			if (!i.MoveForward() || i.Current.Value != "(" || !i.MoveForward())
				throw new LsnrParsingException(i.Current, $"Error parsing {memberTypeName} {memberName}: No parameter list defined", ScriptClass.Path);
			var argTokens = i.SliceWhile(t => t.Value != ")", out bool err);
			if(err)
				throw new LsnrParsingException(i.Current, $"Error parsing {memberTypeName} {memberName}: No parameter list defined", ScriptClass.Path);
			return ScriptClass.ParseParameters(argTokens);
		}

		protected TypeId ParseReturnType(Indexer<Token> index, string memberTypeName, string memberName)
		{
			if (!index.MoveForward() || index.Current.Value == ";" || index.Current.Value == "{") return null;
			TypeId ret = null;
			if (index.Current.Value != "->" || !index.MoveForward())
				throw new LsnrParsingException(index.Current, $"Error parsing {memberTypeName} {memberName}: Expected '->' or end of definition; received '{index.Current.Value}'.",
					ScriptClass.Path);
			if(index.Current.Value == "(")
			{
				if (!index.MoveForward() || index.Current.Value != ")")
					throw new LsnrParsingException(index.Current, "...", ScriptClass.Path);
			}
			else
			{
				var tTokens = index.SliceWhile(t => t.Value != ";" && t.Value != "{", out bool err);
				ret = ScriptClass.ParseTypeId(tTokens, 0, out int x);
				if (ret == null)
					throw new LsnrParsingException(index.Current, "Type not found...", ScriptClass.Path);
			}
			return ret;
		}
	}

	public abstract class ScriptClassStatementRule : ScriptClassRule, IReaderStatementRule
	{
		protected ScriptClassStatementRule(IPreScriptClass scriptClass):base(scriptClass) {}

		public abstract void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> tokens);
	}

	public abstract class ScriptClassBodyRule : ScriptClassRule, IReaderBodyRule
	{
		protected ScriptClassBodyRule(IPreScriptClass scriptClass):base(scriptClass) {}

		public abstract void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> head);
	}

	// NOTE: Put this last in the rule list.
	public sealed class ScriptClassFieldRule : ScriptClassStatementRule
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
			var name = GetName(indexer,"field");
			if (!indexer.MoveForward() || indexer.Current.Value != ":" || !indexer.MoveForward())
				throw new LsnrParsingException(tokens[0], "Invalid field...", ScriptClass.Path);
			var type = ScriptClass.ParseTypeId(tokens, indexer.LengthBehind, out int i);
			if (i < 0)
				throw new LsnrParsingException(tokens[0], "Could not parse type name.", ScriptClass.Path);

			ScriptClass.RegisterField(name, type, mutable);
		}
	}

	public sealed class ScriptClassAbstractMethodRule : ScriptClassStatementRule
	{
		public ScriptClassAbstractMethodRule(IPreScriptClass p) : base(p) { }

		public override bool Check(ISlice<Token> tokens) => tokens.Length > 2
			&& ((tokens[0].Value == "abstract" && tokens[1].Value == "fn")
			|| (tokens[0].Value == "fn" && tokens[1].Value == "abstract"));

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			// abstract fn foo()
			if (tokens.Length < 5)
				throw new LsnrParsingException(tokens[0], "Improperly formatted abstract method definition", ScriptClass.Path);
			var index = new Indexer<Token>(2, tokens);
			var name = GetName(index, "method");
			var args = ParseParameters(index, "method", name);
			var ret = ParseReturnType(index, "method", name);
			ScriptClass.RegisterAbstractMethod(name, ret, args);
		}
	}

	public sealed class ScriptClassMethodRule : ScriptClassBodyRule
	{
		public ScriptClassMethodRule(IPreScriptClass p) : base(p) { }

		public override bool Check(ISlice<Token> head) => head[0].Value == "fn" || head[0].Value == "virtual";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var index = new Indexer<Token>(0, head);
			var virt = false;
			if(index.Current.Value == "virtual")
			{
				virt = true;
				index.MoveForward();
			}
			if (index.Current.Value != "fn")
				throw new LsnrParsingException(index.Current, "Error parsing virtual method", ScriptClass.Path);
			index.MoveForward();
			if(!virt && index.Current.Value == "virtual")
			{
				virt = true;
				index.MoveForward();
			}
			var name = GetName(index, "method");
			var args = ParseParameters(index, "method", name);
			var ret = ParseReturnType(index, "method", name);
			var method = ScriptClass.RegisterMethod(name, ret, args, virt);
			var comp = new ScriptClassMethodComponent(method, body);
			ScriptClass.ParsingProcBodies += comp.OnParsingProcBodies;
		}
	}

	public sealed class ScriptClassEventListenerRule : ScriptClassBodyRule
	{
		public ScriptClassEventListenerRule(IPreScriptClass p):base(p) { }

		public override bool Check(ISlice<Token> head) => head[0].Value == "on";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var index = new Indexer<Token>(1, head);
			var name = GetName(index, "event listener");
			var args = ParseParameters(index, "event listener", name);
			if (index.MoveForward() && index.Current.Value != "{")
				throw new LsnrParsingException(index.Current, "", ScriptClass.Path);
			var listener = ScriptClass.RegisterEventListener(name, args);
			var comp = new ScriptClassEventListenerComponent(listener, body, head[0]);
			ScriptClass.ParsingProcBodies += comp.OnParsingProcBodies;
			ScriptClass.ParsingSignaturesB += comp.Validate;
		}
	}

	public sealed class ScriptClassConstuctorRule : ScriptClassBodyRule
	{
		public ScriptClassConstuctorRule(IPreScriptClass p) : base(p) { }

		public override bool Check(ISlice<Token> head) => head[0].Value == "new";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var index = new Indexer<Token>(0, head);
			var args = ParseParameters(index, "constructor", "new");
			if (index.MoveForward() && index.Current.Value != "{")
				throw new LsnrParsingException(index.Current, "", ScriptClass.Path);
			var constructor = ScriptClass.RegisterConstructor(args);
			if (constructor == null)
				throw new LsnrParsingException(head[0], "A constructor already exists...", ScriptClass.Path);
			var comp = new ScriptClassConstructorComponent(constructor, body);
			ScriptClass.ParsingProcBodies += comp.OnParsingProcBodies;
		}
	}

	public sealed class ScriptClassStateRule : ScriptClassBodyRule
	{
		public ScriptClassStateRule(IPreScriptClass p) : base(p) { }

		public override bool Check(ISlice<Token> head) => head[0].Value == "state" || head[0].Value == "auto";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var index = new Indexer<Token>(0, head);
			var auto = false;
			if(index.Current.Value == "auto")
			{
				auto = true;
				index.MoveForward();
			}
			if (index.Current.Value != "state" || !index.MoveForward())
				throw new LsnrParsingException(index.Current, "Improperly formatted state...", ScriptClass.Path);
			var name = GetName(index, "state");
			var comp = new StateBuilder(ScriptClass, name, auto);
			comp.PreParse(body);
			ScriptClass.ParsingStateSignatures += comp.OnParsingStateSignatures;
			ScriptClass.ParsingProcBodies += comp.OnParsingProcBodies;
		}
	}

	public sealed class ScriptClassConversationRule : ScriptClassBodyRule
	{
		public ScriptClassConversationRule(IPreScriptClass p) : base(p) { }

		public override bool Check(ISlice<Token> head) => head[0].Value == "conversation"
			|| (head[0].Value == "virtual" && head.Length > 1 && head[1].Value == "conversation");

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var index = new Indexer<Token>(0, head);
			var virt = false;
			if(index.Current.Value == "virtual")
			{
				virt = true;
				index.MoveForward();
			}

			if (index.Current.Value != "conversation" || !index.MoveForward())
				throw new LsnrParsingException(index.Current, "Improperly formatted conversation...", ScriptClass.Path);
			var name = index.Current.Value;
			ISlice<Token> args = null;
			if (index.MoveForward())
			{
				if (index.Current.Value != "(")
					throw LsnrParsingException.UnexpectedToken(index.Current, "( or {", ScriptClass.Path);
				if (!index.MoveForward())
					throw new LsnrParsingException(head[0], "Improperly formatted conversation...", ScriptClass.Path);
				args = index.SliceWhile(t => t.Value != ")", out bool err);
				if (err)
					throw new LsnrParsingException(index.Current, $"Error parsing conversation {name}: No parameter list defined", ScriptClass.Path);
			}
			var conv = new ConversationBuilder(ScriptClass, name, args, virt);
			var reader = new ConversationReader(conv, body);
			ScriptClass.ParsingSignaturesB += conv.OnParsingSignatures;
			ScriptClass.ParsingProcBodies += (_) => conv.Parse();
			reader.Read();
		}
	}
}
