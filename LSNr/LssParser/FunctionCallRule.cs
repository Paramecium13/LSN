﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	public class FunctionCallRule : IExpressionRule
	{
		public uint Priority => ExpressionRulePriorities.MemberAccess;

		public bool CheckToken(Token token, IPreScript script)
			=> token.Type == TokenType.Identifier && script.CheckSymbol(token.Value) == SymbolType.Function;

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var fn = script.GetFunction(tokens[index].Value);
			if(fn.Parameters.Count == 0)
			{
				var next = index + 1;
				if(next < tokens.Count && tokens[next].Value == "(")
				{
					++next;
					if (next >= tokens.Count)
						throw new LsnrParsingException(tokens[index + 1], $"Missing closing parenthesis for function call '{fn.Name}'.", script.Path);
					if (tokens[next].Value != ")")
						throw LsnrParsingException.UnexpectedToken(tokens[next], ")", script.Path);
					++next;
				}
				return (new FunctionCall(fn, new IExpression[0]), next, 0);
			}

			var (args, nextIndex) = Create.CreateArgs(index + 1, tokens, script, substitutions);

			return (new FunctionCall(fn, args), nextIndex, 0);
		}
	}
}
