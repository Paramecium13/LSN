using LsnCore.Statements;
using LsnCore.Utilities;

namespace LSNr.Statements
{
	public interface IStatementRule
	{
		bool PreCheck(Token t);
		bool Check(ISlice<Token> tokens, IPreScript script);
		Statement Apply(ISlice<Token> tokens, IPreScript script);
	}
}
