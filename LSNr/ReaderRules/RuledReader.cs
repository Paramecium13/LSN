using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Utilities;

namespace LSNr.ReaderRules
{

	[Serializable]
	public class NoValidRuleException : Exception
	{
		public NoValidRuleException() { }
		public NoValidRuleException(string message) : base(message) { }
		public NoValidRuleException(string message, Exception inner) : base(message, inner) { }
		protected NoValidRuleException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	abstract class RuledReader<TStatementRule, TBodyRule> : ReaderBase
		where TStatementRule : IReaderStatementRule
		where TBodyRule : IReaderBodyRule
	{
		protected abstract IEnumerable<TStatementRule> StatementRules { get; }
		protected abstract IEnumerable<TBodyRule> BodyRules { get; }

		protected RuledReader(ISlice<Token> tokens) : base(tokens) { }

		protected override void OnReadBody(ISlice<Token> headTokens, ISlice<Token> bodyTokens, ISlice<Token>[] attributes)
		{
			foreach (var rule in BodyRules)
			{
				if(rule.Check(headTokens))
				{
					rule.Apply(headTokens, bodyTokens, attributes);
					return;
				}
			}
			throw new NoValidRuleException();
		}

		protected override void OnReadStatement(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			foreach (var rule in StatementRules)
			{
				if (rule.Check(tokens))
				{
					rule.Apply(tokens, attributes);
					return;
				}
			}
			throw new NoValidRuleException();
		}
	}
}
