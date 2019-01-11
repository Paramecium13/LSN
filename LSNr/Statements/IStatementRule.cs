using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Utilities;
using System.Collections.Generic;

namespace LSNr.Statements
{
	public interface IStatementRule
	{
		int Order { get; }
		bool PreCheck(Token t);
		bool Check(ISlice<Token> tokens, IPreScript script);
		Statement Apply(ISlice<Token> tokens, IPreScript script);
	}

	public interface ISemiParsedStatementRule
	{
		int Order { get; }
		bool Check(ISlice<Token> tokens, IReadOnlyDictionary<Token, IExpression> substitutions, IPreScript script);
		Statement Apply(ISlice<Token> tokens, IReadOnlyDictionary<Token, IExpression> substitutions, IPreScript script);
	}

	public static class StatementRuleOrders
	{
		public const int Base = 0;

		public const int GiveGold = 100;

		public const int GiveItem = 101;

		public const int Reassign = 10_000;
	}
}
