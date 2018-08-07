using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.LssParser
{
	public interface IExpressionRule
	{
		uint Priority { get; }

		/// <summary>
		/// Does this token indicate that this expression applies?
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="script">The context of the expression.</param>
		/// <returns>Does the rule possibly apply?</returns>
		bool CheckToken(Token token, IPreScript script);

		/// <summary>
		/// Performs a second, more detailed check to see if this rule applies.
		/// </summary>
		/// <param name="index">The index of the token that was passed to CheckToken.</param>
		/// <param name="tokens">The tokens that make up the entire expression.</param>
		/// <param name="script">The context of the expression.</param>
		/// <param name="substitutions">The current substitutions.</param>
		/// <returns>Does the rule apply?</returns>
		bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions);

		/// <summary>
		/// Create the expression.
		/// </summary>
		/// <param name="index">The index of the token that was passed to CheckToken.</param>
		/// <param name="tokens">The tokens that make up the entire expression.</param>
		/// <param name="script">The context of the expression.</param>
		/// <param name="substitutions">The current substitutions.</param>
		/// <returns>The expression; the index of the next token after the new expression;
		/// the number of tokens to the left that are part of this expression.
		/// Tokens between [index - numTokensToRemoveFromLeft, indexOfNextToken) are part of the new expression and will be removed.</returns>
		(IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token,IExpression> substitutions);
	}
}
