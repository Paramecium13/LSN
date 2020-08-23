using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	/// <summary>
	/// The type of syntactical feature a <see cref="Token"/> represents.
	/// </summary>
	public enum TokenType
	{ 
		Ambiguous,
		/// <summary>
		/// A token that represents an assignment operator, such as <c>=</c> or <c>+=</c>.
		/// </summary>
		Assignment,
		/// <summary>
		/// A token that represents a <see langword="double"/>.
		/// </summary>
		Float,
		
		/// <summary>
		/// A token that represents an identifier, such as a variable or method. This is the default value when no other type applies...
		/// </summary>
		Identifier,
		
		/// <summary>
		/// A token that represents an <see langword="int"/>.
		/// </summary>
		Integer,
		
		/// <summary>
		/// A token that represents a keyword.
		/// </summary>
		Keyword,
		
		/// <summary>
		/// A token that represents an operator.
		/// </summary>
		Operator,
		
		/// <summary>
		/// A token that represents a <see langword="string"/>.
		/// </summary>
		String,

		/// <summary>
		/// A token that represents a syntax symbol such as <c>{</c>, <c>(</c>, or <c>;</c>.
		/// </summary>
		SyntaxSymbol,

		/// <summary>
		/// A token that represents a GameValue (not yet implemented).
		/// </summary>
		GameValue,
		
		/// <summary>
		/// A substitution token created by the <see cref="LssParser.ExpressionParser"/>.
		/// </summary>
		Substitution
	}

	/// <summary>
	/// A token in the source code
	/// </summary>
	public sealed class Token : IEquatable<Token>
	{
		/// <summary>
		/// The type of token this is.
		/// </summary>
		public readonly TokenType Type;

		/// <summary>
		/// The value of a token whose type is <see cref="TokenType.Integer"/>.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
		public readonly int IntValue;

		/// <summary>
		/// The value of a token whose type is <see cref="TokenType.Float"/>.
		/// </summary>
		public readonly double DoubleValue = double.NaN;

		/// <summary>
		/// The line number where the token occured.
		/// </summary>
		public int LineNumber { get; }

		public string Value { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Token"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="lineNumber">The line number.</param>
		/// <param name="type">The type.</param>
		public Token(string value, int lineNumber, TokenType type)
		{
			Value = value; LineNumber = lineNumber; Type = type;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Token"/> class of type <see cref="TokenType.Float"/>.
		/// </summary>
		/// <param name="lineNumber">The line number.</param>
		/// <param name="value">The value.</param>
		public Token(int lineNumber, double value):this(value.ToString(),lineNumber,TokenType.Float)
		{
			DoubleValue = value;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Token"/> class of type <see cref="TokenType.Integer"/>.
		/// </summary>
		/// <param name="lineNumber">The line number.</param>
		/// <param name="value">The value.</param>
		public Token(int lineNumber, int value):this(value.ToString(),lineNumber,TokenType.Integer)
		{
			IntValue = value;
		}

		/// <inheritdoc/>
		public bool Equals(Token other)
		{
			return other?.Value == Value;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is Token o))
				return false;
			return o.Value == Value;
		}

		/// <inheritdoc/>
		public override int GetHashCode() => Value.GetHashCode();

		/// <inheritdoc/>
		public override string ToString() => Value;
	}
}
