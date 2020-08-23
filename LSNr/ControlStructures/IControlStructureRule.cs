using LsnCore.ControlStructures;
using LsnCore.Utilities;

namespace LSNr.ControlStructures
{
	/// <summary>
	/// A rule for parsing control structures.
	/// </summary>
	public abstract class ControlStructureRule
	{
		/// <summary>
		/// The order in which this rule is checked, lower ordered rules are checked first.
		/// </summary>
		public virtual int Order => ControlStructureRuleOrders.Base;

		/// <summary>
		/// The initial check method. Checks if <paramref name="token"/> is a valid first <see cref="Token"/> for this control structure.
		/// If this passes, then <see cref="Check(ISlice{Token}, IPreScript)"/> is called.
		/// </summary>
		/// <param name="token">The first token.</param>
		public abstract bool PreCheck(Token token);

		/// <summary>
		/// Checks if <paramref name="tokens"/> is a valid head for this control structure.
		/// </summary>
		/// <param name="tokens">The tokens that make up the head of a control structure.</param>
		/// <param name="script">The script.</param>
		/// <returns></returns>
		public abstract bool Check(ISlice<Token> tokens, IPreScript script);

		/// <summary>
		/// Creates this control structure with <paramref name="head"/> and <paramref name="body"/>.
		/// </summary>
		/// <param name="head">The head.</param>
		/// <param name="body">The body.</param>
		/// <param name="script">The script.</param>
		/// <returns></returns>
		public abstract ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script);
	}

	public static class ControlStructureRuleOrders
	{
		public static readonly int Base = 0;

		public static readonly int ElsIf = 100;

		public static readonly int Else = 101;
	}
}
