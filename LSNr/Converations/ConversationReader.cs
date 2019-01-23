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
		bool NodeExists(string name);
	}

	sealed class ConversationReader : RuledReader<ConversationStatementRule, ConversationBodyRule>
	{
		protected override IEnumerable<ConversationStatementRule> StatementRules { get; }

		protected override IEnumerable<ConversationBodyRule> BodyRules { get; }

		public ConversationReader(ISlice<Token> tokens) : base(tokens) { }

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes){ }
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
			var i = 0;
			var first = false;
			if(head[i].Value == "auto")
			{
				first = true;
				++i;
			}
			var name = head[i].Value;
			var builder = new NodeBuilder(Conversation, name);
			var reader = new NodeReader(builder, body);
			reader.Read();
			Conversation.RegisterNode(builder, first);
			// ToDo: Hook-up events.
		}
	}
}
