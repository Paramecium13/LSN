using LsnCore;
using LsnCore.Utilities;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Converations
{
	interface IConversation : ITypeContainer
	{
		ISlice<Token> StartTokens { get; set; }
		void RegisterNode(INode node, bool first);
		void RegisterConvVar(IConversationVariable convVar);
		bool NodeExists(string name);

		Variable JumpTargetVariable { get; }
		string Path { get; }
	}

	sealed class ConversationReader : RuledReader<ConversationStatementRule, ConversationBodyRule>
	{
		protected override IEnumerable<ConversationStatementRule> StatementRules { get; }

		protected override IEnumerable<ConversationBodyRule> BodyRules { get; }

		public ConversationReader(IConversation conversation, ISlice<Token> tokens) : base(tokens)
		{
			StatementRules = new ConversationStatementRule[] {
				new ConversationVariableRule(conversation)
			};
			BodyRules = new ConversationBodyRule[] {
				new NodeRule(conversation),
				new ConversationStartRule(conversation)
			};
		}

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes) { }

		public void Read() => ReadTokens();
	}

	abstract class ConversationStatementRule : IReaderStatementRule
	{
		protected readonly IConversation Conversation;
		protected ConversationStatementRule(IConversation c) { Conversation = c; }

		public abstract void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> tokens);
	}

	abstract class ConversationBodyRule : IReaderBodyRule
	{
		protected readonly IConversation Conversation;
		protected ConversationBodyRule(IConversation c) { Conversation = c; }

		public abstract void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> head);
	}

	sealed class NodeRule : ConversationBodyRule
	{
		public NodeRule(IConversation c) : base(c) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Type == TokenType.Keyword && (head[0].Value == "node"
			|| (head[0].Value == "auto" && head.Length > 1 && head[1].Type == TokenType.Keyword && head[1].Value == "node"));

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var i = 1;
			var first = false;
			if (head[0].Value == "auto")
			{
				first = true;
				++i;
			}
			var name = head[i].Value;
			var builder = new NodeBuilder((ConversationBuilder)Conversation, name);
			var reader = new NodeReader(builder, body);
			reader.Read();
			Conversation.RegisterNode(builder, first);
			// ToDo: Hook-up events ??
		}
	}

	sealed class ConversationStartRule : ConversationBodyRule
	{
		public ConversationStartRule(IConversation conversation) : base(conversation) { }

		public override bool Check(ISlice<Token> head)
		{
			if (head[0].Value == "start") return true;
			if (head.Length < 2)
				return false;
			if (head[0].Value == "fn" && head[1].Value == "start")
				return true;
			return false;
		}

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			if (head[0].Value == "fn" && head[1].Value == "start")
			{
				// ToDo: Make sure it has no parameters, return type, or junk.
			}
			if (Conversation.StartTokens != null)
				throw new LsnrParsingException(head[0], "Conversation can't have more than one start block...", Conversation.Path);
			Conversation.StartTokens = body;
		}
	}

	sealed class ConversationVariableRule : ConversationStatementRule
	{
		public ConversationVariableRule(IConversation c) : base(c) { }

		public override bool Check(ISlice<Token> tokens) => tokens[0].Value == "let";

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			// let x = y;
			if (tokens.Length < 4)
				throw new LsnrParsingException(tokens[0], "Improperly formatted conversation variable", Conversation.Path);
			var mutable = false;
			var nameIndex = 1;
			if (tokens[1].Value == "mut")
			{
				mutable = true;
				nameIndex++;
			}
			var name = tokens[nameIndex].Value;
			if (tokens[nameIndex].Type != TokenType.Identifier)
				throw new LsnrParsingException(tokens[0], $"Cannot create a variable named '{name}'.", Conversation.Path);
			if (tokens[++nameIndex].Value != "=")
				throw LsnrParsingException.UnexpectedToken(tokens[nameIndex], "=", Conversation.Path);
			nameIndex++;
			var expr = tokens.CreateSliceSkipTake(nameIndex, tokens.Length - nameIndex - 1);
			Conversation.RegisterConvVar(new ConversationVariable(name, mutable, expr));
		}
	}
}
