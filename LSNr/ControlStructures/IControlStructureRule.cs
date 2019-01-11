using LsnCore.ControlStructures;
using LsnCore.Utilities;

namespace LSNr.ControlStructures
{
	public abstract class ControlStructureRule
	{
		public virtual int Order => ControlStructureRuleOrders.Base;
		public abstract bool PreCheck(Token t);
		public abstract bool Check(ISlice<Token> tokens, IPreScript script);
		public abstract ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script);
	}

	public static class ControlStructureRuleOrders
	{
		public static readonly int Base = 0;

		public static readonly int ElsIf = 100;

		public static readonly int Else = 101;
	}
}
