using LsnCore;
using LsnCore.Expressions;
using LsnCore.Utilities;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Converations
{
	public interface IBranch : ITypeContainer
	{
		string Name { get; }
		IExpression Condition { get; set; }
		IExpression Prompt { get; set; }
		ISlice<Token> ActionTokens { get; set; }
		string Path { get; }

		event Action<IPreScript> ParsingBodies;
		void Parse(IPreScript script);
	}

	public abstract class BranchStatementRule : IReaderStatementRule
	{
		protected readonly IBranch Branch;
		protected BranchStatementRule(IBranch branch) { Branch = branch; }

		public abstract bool Check(ISlice<Token> tokens);
		public abstract void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
	}

	public abstract class BranchBodyRule : IReaderBodyRule
	{
		protected readonly IBranch Branch;
		protected BranchBodyRule(IBranch branch) { Branch = branch; }

		public abstract bool Check(ISlice<Token> head);
		public abstract void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
	}

	public sealed class BranchReader : RuledReader<BranchStatementRule, BranchBodyRule>
	{
		protected override IEnumerable<BranchStatementRule> StatementRules { get; }

		protected override IEnumerable<BranchBodyRule> BodyRules { get; }

		public BranchReader(IBranch branch, ISlice<Token> tokens) : base(tokens)
		{
			StatementRules = new BranchStatementRule[]
			{
				new PromptStatementRule(branch),
				new ConditionStatementRule(branch)
			};
			BodyRules = new BranchBodyRule[] { new ActionBodyRule(branch) };
		}

		public void Read() => ReadTokens();

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes) { }
	}

	sealed class PromptStatementRule : BranchStatementRule
	{
		public PromptStatementRule(IBranch b) : base(b) { }

		public override bool Check(ISlice<Token> tokens)
			=> tokens[0].Value == "prompt" && tokens.Length >= 3
			&& (tokens[1].Value == "=" || tokens[1].Value == ":" || tokens[1].Value == "->" || tokens[1].Value == "=>");

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			var exprTokens = tokens.CreateSliceSkipTake(2, tokens.Length - 3);

			Branch.ParsingBodies += (script) =>
			{
				if (Branch.Prompt != null)
					throw new LsnrParsingException(exprTokens[0], $"Error: Branch {Branch.Name} has more than one prompt.", script.Path);
				Branch.Prompt = Create.Express(exprTokens, script);
			};
		}
	}

	sealed class ConditionStatementRule : BranchStatementRule
	{
		public ConditionStatementRule(IBranch b) : base(b) { }

		public override bool Check(ISlice<Token> tokens)
			=> tokens[0].Value == "when" && tokens.Length >= 3
			&& (tokens[1].Value == "=" || tokens[1].Value == ":" || tokens[1].Value == "->" || tokens[1].Value == "=>");

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			var exprTokens = tokens.CreateSliceSkipTake(2, tokens.Length - 3); ;

			Branch.ParsingBodies += (script) =>
			{
				if (Branch.Condition != null)
					throw new LsnrParsingException(exprTokens[0], $"Error: Branch {Branch.Name} has more than one prompt.", script.Path);
				Branch.Condition = Create.Express(exprTokens, script);
			};
		}
	}

	sealed class ActionBodyRule : BranchBodyRule
	{
		public ActionBodyRule(IBranch b) : base(b) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "action" || (head[0].Value == "fn" && head.Length >= 2 && head[1].Value == "action");

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			if (head.Length > 2)
			{
				// ToDo: Make sure it doesn't have any return type, parameters, or junk
			}
			if (Branch.ActionTokens != null)
				throw new LsnrParsingException(head[0], $"Branch {Branch.Name} has more than one action.", Branch.Path);
			Branch.ActionTokens = body;
		}
	}
}
