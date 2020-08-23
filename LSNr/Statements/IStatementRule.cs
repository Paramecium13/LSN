using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Utilities;
using System.Collections.Generic;

namespace LSNr.Statements
{
	/// <summary>
	/// A rule for parsing statements.
	/// </summary>
	public interface IStatementRule
	{
		/// <summary>
		/// The order in which this rule is checked. Rules with a lower <see cref="Order"/> are checked first.
		/// </summary>
		int Order { get; }

		/// <summary>
		/// The initial check method. Checks if <paramref name="token"/> is a valid first <see cref="Token"/> for this statement.
		/// If this passes, then <see cref="Check(ISlice{Token}, IPreScript)"/> is called.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns></returns>
		bool PreCheck(Token token);

		/// <summary>
		/// Checks if the specified tokens are valid for this statement.
		/// </summary>
		/// <param name="tokens">The tokens.</param>
		/// <param name="script">The script.</param>
		/// <returns></returns>
		bool Check(ISlice<Token> tokens, IPreScript script);

		/// <summary>
		/// Creates a statement from <paramref name="tokens"/>.
		/// </summary>
		/// <param name="tokens">The tokens.</param>
		/// <param name="script">The script.</param>
		/// <returns></returns>
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
