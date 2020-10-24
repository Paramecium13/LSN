using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Utilities;
using LSNr.CodeGeneration;
using LSNr.Statements;
using Syroot.BinaryData;

namespace LSNr.Converations
{
	internal class ConversationReturnStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t)
			=> t.Value == "return";

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			if (tokens.Length > 1)
				throw new LsnrParsingException(tokens[1],"Cannot return a value from a conversation...",script.Path);
			return new JumpToTargetStatement();
		}

		public bool Check(ISlice<Token> tokens, IPreScript script) => true;
	}

	internal class EndConversationStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t)
			=> t.Value == "endconversation" || t.Value == "end";

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			if ((tokens[0].Value == "end" && tokens.Length > 2) || (tokens[0].Value != "end" && tokens.Length > 1))
				throw new LsnrParsingException(tokens[1], "Cannot return a value from a conversation...", script.Path);
			return new ReturnStatement(null);
		}

		public bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens[0].Value == "endconversation" || (tokens.Length > 1 && tokens[1].Value == "conversation");
	}

	internal class SetNodeStatement : Statement
	{
		public readonly string Node;

		public SetNodeStatement(string node)
		{
			Node = node;
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			throw new InvalidOperationException();
		}

		/// <inheritdoc />
		protected virtual IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context)
		{
			throw new InvalidOperationException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr) { }

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}
	}

	internal class SetNodeStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t)
			=> t.Value == "setnode" || t.Value == "set";

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			// set node foo
			if ((tokens[0].Value == "set" && tokens.Length > 3) ||( tokens[0].Value != "set" && tokens.Length > 2))
				throw new LsnrParsingException(tokens[1], "Improperly formatted Set Node statement...", script.Path);
			var index = tokens[0].Value == "set" ? 2 : 1;
			var name = tokens[index].Value;

			return new SetNodeStatement(name + " Start");
		}

		public bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens[0].Value == "setnode" || (tokens.Length > 1 && tokens[1].Value == "node");
	}
}
