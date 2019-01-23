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
	interface INode : ITypeContainer
	{
		bool BranchExists(string name);
		string Name { get; }
		bool NodeExists(string name);
	}

	sealed class NodeReader : RuledReader<NodeStatementRule, NodeBodyRule>
	{
		protected override IEnumerable<NodeStatementRule> StatementRules { get; }

		protected override IEnumerable<NodeBodyRule> BodyRules { get; }

		public NodeReader(ISlice<Token> tokens) : base(tokens) { }

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes) { }
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
			throw new NotImplementedException();
		}
	}
}
