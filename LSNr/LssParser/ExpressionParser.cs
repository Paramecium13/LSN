using LsnCore.Expressions;
using LsnCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.LssParser
{
	public class ExpressionParser
	{
		private static IExpressionRule[][] Rules;

		private const string SUB = "Ƨ";

		private readonly Dictionary<Token, IExpression> Substitutions;
#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly IReadOnlyList<Token> InitialTokens;
#endif
		private List<Token> CurrentTokens;
		private int SubCount;
		private readonly IPreScript Script;

		private ExpressionParser(IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
#if DEBUG
			InitialTokens = tokens;
#endif
			CurrentTokens = new List<Token>(tokens);
			Script = script;
			Substitutions = new Dictionary<Token, IExpression>();
			if (substitutions != null)
			{
				SubCount = substitutions.Count;
				foreach (var pair in substitutions)
				{
					Substitutions.Add(pair.Key, pair.Value);
				}
			}
		}

		private IExpression Parse()
		{
			foreach (var level in Rules)
			{
				ApplyRuleLevel(level);
				if (CurrentTokens.Count == 1 && CurrentTokens[0].Value.StartsWith(SUB, StringComparison.Ordinal))
					break;
			}

			if (CurrentTokens.Count != 1)
				throw LsnrParsingException.UnexpectedToken(CurrentTokens[1],"end of expression",Script.Path);

			return Substitutions[CurrentTokens[0]];
		}

		private (Token[] tokens, IReadOnlyDictionary<Token, IExpression> substitutions)
			MultiParse()
		{
			foreach (var level in Rules)
			{
				ApplyRuleLevel(level);
				if (CurrentTokens.Count == 1) break;
			}

			return
			(
				CurrentTokens.ToArray(),
				Substitutions
					.Where(p => CurrentTokens.Contains(p.Key))
					.ToDictionary(p => p.Key, p => p.Value)
			);
		}

		private void ApplyRuleLevel(IExpressionRule[] rules)
		{
			var i = 0;
			while (i < CurrentTokens.Count)
			{
				foreach (var rule in rules)
				{
					if (!rule.CheckToken(CurrentTokens[i], Script) ||
					    !rule.CheckContext(i, CurrentTokens, Script, Substitutions)) continue;
					var (expression, indexOfNextToken, numTokensToRemoveFromLeft) = rule.CreateExpression(i, CurrentTokens, Script, Substitutions);
					var t = new Token(SUB + SubCount++, -1, TokenType.Substitution);
					Substitutions.Add(t, expression);

					CurrentTokens.Insert(indexOfNextToken, t);
					var c = numTokensToRemoveFromLeft + (indexOfNextToken-i);
					CurrentTokens.RemoveRange(i - numTokensToRemoveFromLeft, c);
					i -= numTokensToRemoveFromLeft;
					break;
				}
				++i;
			}
		}

		public static void DefaultSetUp()
		{
			SetupRules(new[]
			{
				ConstantRule.Rule,
				SelfMethodCallRule.Rule,
				new MemberAccessRule(),
				new PropertyFieldRule(),
				new VariableExpressionRule(),
				BinaryExpressionRule.Difference,
				BinaryExpressionRule.Equal,
				BinaryExpressionRule.GreaterThan,
				BinaryExpressionRule.GreaterThanOrEqual,
				BinaryExpressionRule.LessThan,
				BinaryExpressionRule.LessThanOrEqual,
				BinaryExpressionRule.LogicalAnd,
				BinaryExpressionRule.LogicalOr,
				BinaryExpressionRule.Modulus,
				BinaryExpressionRule.NotEqual,
				BinaryExpressionRule.Product,
				BinaryExpressionRule.Quotient,
				BinaryExpressionRule.Sum,
				BinaryExpressionRule.Power,
				new ParenthesisRule(),
				new FunctionCallRule(),
				new IndexerRule(),
				new ConstructorRule(),
				UnaryExpressionRule.NegationRule,
				UnaryExpressionRule.NotRule,
				RangeExpressionRule.Rule,
				SomeRule.Rule
			});
		}

		public static void SetupRules(IExpressionRule[] rules)
		{
			Rules = rules
				.GroupBy(r => r.Priority)
				.Select(g => new Tuple<uint, IExpressionRule[]>(g.Key, g.ToArray()))
				.OrderByDescending(g => g.Item1)
				.Select(g => g.Item2)
				.ToArray();
		}

		public static IExpression Parse(IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions = null)
			=> new ExpressionParser(tokens,script,substitutions).Parse();

		public static (Token[] tokens, IReadOnlyDictionary<Token, IExpression> substitutions)
			MultiParse(IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions = null)
			=> new ExpressionParser(tokens, script, substitutions).MultiParse();
	}
}
