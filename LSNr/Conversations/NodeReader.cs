using LsnCore;
using LsnCore.Utilities;
using LSNr.Optimization;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Conversations
{
	internal interface INode : ITypeContainer
	{
		string Name { get; }
		string Path { get; }
		ISlice<Token> StartBlockTokens { get; set; }
		IConversation Conversation { get; }

		bool NodeExists(string name);
		bool BranchExists(string name);
		void RegisterBranch(IBranch branch);

		void Parse(ComponentFlattener flattener, IScope scope);
	}

	sealed class NodeReader : RuledReader<NodeStatementRule, NodeBodyRule>
	{
		protected override IEnumerable<NodeStatementRule> StatementRules { get; }

		protected override IEnumerable<NodeBodyRule> BodyRules { get; }

		public NodeReader(INode node, ISlice<Token> tokens) : base(tokens)
		{
			StatementRules = Array.Empty<NodeStatementRule>();
			BodyRules = new NodeBodyRule[] { new BranchRule(node), new NodeStartRule(node) };
		}

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes) { }

		public void Read()
		{
			ReadTokens();
		}
	}

	abstract class NodeStatementRule : IReaderStatementRule
	{
		protected readonly INode Node;
		protected NodeStatementRule(INode n) { Node = n; }

		public abstract void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> tokens);
	}

	abstract class NodeBodyRule : IReaderBodyRule
	{
		protected readonly INode Node;
		protected NodeBodyRule(INode n) { Node = n; }

		public abstract void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> head);
	}

	sealed class BranchRule : NodeBodyRule
	{
		public BranchRule(INode n) : base(n) { }

		public override bool Check(ISlice<Token> head) => head[0].Type == TokenType.Keyword && head[0].Value == "branch";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			if (head.Length != 3 || head[1].Type != TokenType.Identifier)
				throw new LsnrParsingException(head[2], "Improperly formatted branch", Node.Path);
			var name = head[1].Value;
			if (Node.BranchExists(name))
				throw new LsnrParsingException(head[1], $"Node {Node.Name} already has a branch named {name}.", Node.Path);
			var branch = new BranchBuilder(name, Node.Conversation);
			var reader = new BranchReader(branch, body);
			reader.Read();
			Node.RegisterBranch(branch);
		}
	}

	sealed class NodeStartRule : NodeBodyRule
	{
		public NodeStartRule(INode node) : base(node) { }

		public override bool Check(ISlice<Token> head)
		{
			if (head[0].Value == "start") return true;
			if (head.Length < 2)
				return false;
			return head[0].Value == "fn" && head[1].Value == "start";
		}

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			if (head[0].Value == "fn" && head[1].Value == "start")
			{
				// ToDo: Make sure it has no parameters, return type, or junk.
			}
			if (Node.StartBlockTokens != null)
				throw new LsnrParsingException(head[0], "Conversation can't have more than one start block...", Node.Path);
			Node.StartBlockTokens = body;
		}
	}
}
