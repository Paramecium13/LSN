using LsnCore.Statements;
using LsnCore.Utilities;

namespace LSNr.Statements
{
	public interface IStatementRule
	{
		int Order { get; }
		bool PreCheck(Token t);
		bool Check(ISlice<Token> tokens, IPreScript script);
		Statement Apply(ISlice<Token> tokens, IPreScript script);
	}

	public static class StatementRuleOrders
	{
		public const int Base = 0;

		public const int Last = int.MaxValue;
	}
}
