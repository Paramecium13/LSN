using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public enum TokenType { Ambiguous, Assignment, Float, Identifier, Integer, Keyword, Operator, String, SyntaxSymbol, GameValue,
		Substitution
	}

	public sealed class Token
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

		public bool Equals(Token other)
		{
			return other.Value == Value;
		}

		public override bool Equals(object obj)
		{
			var o = obj as Token;
			if (o == null)
				return false;
			return o.Value == Value;
		}

		public override int GetHashCode() => Value.GetHashCode();

		public override string ToString() => Value;
	}

	public static class TokenExtensions
	{

		public static bool Equals(Token a, Token b)
		{
			// Putting Value comparison first to improve performance by short circuiting becaues it is
			// most likely to fail and I think it is easier to do.
			return a.Value == b.Value && a.GetType().Equals(b.GetType());
		}

	}

}
