using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.LssParser
{
	public class ExpressionParser
	{
		private static Tuple<uint, IExpressionRule[]>[] Rules;

		private const string SUB = "Ƨ";

		private readonly Dictionary<Token, IExpression> Substitutions;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly Token[] InitialTokens;
		private List<Token> CurrentTokens;
		private int SubCount;
		private readonly IPreScript Script;

		private ExpressionParser(Token[] tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			InitialTokens = tokens;
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
				ApplyRuleLevel(level.Item2);
				if (CurrentTokens.Count == 1)
					break;
			}

			if (CurrentTokens.Count != 1)
				throw new ApplicationException();

			return Substitutions[CurrentTokens[0]];
		}

		private void ApplyRuleLevel(IExpressionRule[] rules)
		{
			var i = 0;
			while (i < CurrentTokens.Count)
			{
				foreach (var rule in rules)
				{
					if (rule.CheckToken(CurrentTokens[i], Script)
						&& rule.CheckContext(i, CurrentTokens, Script, Substitutions))
					{
						var x = rule.CreateExpression(i, CurrentTokens, Script, Substitutions);
						var t = new Token(SUB + SubCount++, -1, TokenType.Substitution);
						Substitutions.Add(t, x.expression);

						CurrentTokens.Insert(x.indexOfNextToken, t);
						var c = x.numTokensToRemoveFromLeft + (x.indexOfNextToken-i);
						CurrentTokens.RemoveRange(i - x.numTokensToRemoveFromLeft, c);
						i -= x.numTokensToRemoveFromLeft;
						break;
					}
				}
				++i;
			}
		}

		public static void DefaultSetUp()
		{
			SetupRules(new IExpressionRule[]
				{ ConstantRule.Rule,
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
				UnaryExpressionRule.NotRule});
		}

		public static void SetupRules(IExpressionRule[] rules)
		{
			Rules = rules
				.GroupBy(r => r.Priority)
				.Select(g => new Tuple<uint, IExpressionRule[]>(g.Key, g.ToArray()))
				.OrderByDescending(g => g.Item1)
				.ToArray();
		}

		public static IExpression Parse(Token[] tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions = null)
			=> new ExpressionParser(tokens,script,substitutions).Parse();
	}
}
