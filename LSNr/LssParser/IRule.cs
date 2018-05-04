using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.LssParser
{
	public interface IRule
	{
		uint Priority { get; }

		/// <summary>
		/// IsInitializingToken...
		/// </summary>
		/// <param name="token"></param>
		/// <param name="script"></param>
		/// <returns></returns>
		bool CheckToken(Token token, IPreScript script);

		bool CheckContext(int index, Token[] tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions);

		(IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, Token[] tokens, IPreScript script, IReadOnlyDictionary<Token,IExpression> substitutions);
	}
}
