using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
{
	public struct FloatToken : IToken
	{
		public string Value { get; private set; }
		public readonly double DVal;

		public FloatToken(string s)
		{
			Value = s;
			DVal = Double.Parse(s);
		}

		public FloatToken(double d)
		{
			Value = d.ToString();
			DVal = d;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
