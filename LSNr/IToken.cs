using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens
{
	public enum TokenType { Ambiguous, Assignment, Float, Identifier, Integer, Keyword, Operator, String, Substitution, SyntaxSymbol}

	public interface IToken : IEquatable<IToken>
	{
		string Value { get; }
		int LineNumber { get; }
	}

	public sealed class Token : IToken
	{
		private readonly int _LineNumber;
		private readonly string _Value;

		public readonly TokenType Type;
		public readonly int IntValue;
		public readonly double DoubleValue = double.NaN;

		public int LineNumber => _LineNumber;
		public string Value => _Value;

		public Token(string value, int lineNumber, TokenType type)
		{
			_Value = value; _LineNumber = lineNumber; Type = type;
		}

		public Token(int lineNumber, double value):this(value.ToString(),lineNumber,TokenType.Float)
		{
			DoubleValue = value;
		}


		public Token(int lineNumber, int value):this(value.ToString(),lineNumber,TokenType.Integer)
		{
			IntValue = value;
		}

		public bool Equals(IToken other)
		{
			return other.Value == Value;
		}
	}

	public static class TokenExtensions
	{

		public static bool Equals(IToken a, IToken b)
		{
			// Putting Value comparison first to improve performance by short circuiting becaues it is
			// most likely to fail and I think it is easier to do.
			return a.Value == b.Value && a.GetType().Equals(b.GetType());
		}

	}

}
