using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	class SelfMethodCallRule : IExpressionRule
	{
		internal static readonly SelfMethodCallRule Rule = new SelfMethodCallRule();

		SelfMethodCallRule() { }

		public uint Priority => ExpressionRulePriorities.MemberAccess;

		// if this was preceded by a '.', MemberAccessRule would have caught it.
		public bool CheckToken(Token token, IPreScript script)
			=> script.CheckSymbol(token.Value) == SymbolType.ScriptClassMethod && script is PreScriptClassFunction preFn && preFn.Parent.MethodExists(token.Value);

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var args = new IExpression[] { new VariableExpression(0) };
			var preFn = script as PreScriptClassFunction;
			var method = preFn.Parent.Id.Type.Methods[tokens[index].Value];
			var nextIndex = index + 1;
			if (method.Parameters.Count == 1 && index + 2 < tokens.Count && tokens[index + 1].Value == "(")
			{
				if (tokens[index + 2].Value != ")")
					throw new LsnrParsingException(tokens[index + 2], "...", script.Path);
				nextIndex = index + 3; // Skip over the parenthesis.
			}
			else if (method.Parameters.Count > 1)
			{
				var x = Create.CreateArgList(index + 1, tokens, script);
				var argTokens = x.argTokens;
				nextIndex = x.indexOfNextToken;

				var a = new List<IExpression>(method.Parameters.Count);
				a.Add(args[0]);
				a.AddRange(argTokens.Select(ar => ExpressionParser.Parse(ar, script, substitutions)));
				args = a.ToArray();
			}
			return (method.CreateMethodCall(args), nextIndex, 0);
		}
	}
}
