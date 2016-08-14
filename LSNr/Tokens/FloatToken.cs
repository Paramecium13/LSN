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
		public int LineNumber { get; }
		public readonly double DVal;

		public FloatToken(string s, int line = 0)
		{
			Value = s; LineNumber = line;
			DVal = double.Parse(s);
		}

		public FloatToken(double d, int line = 0)
		{
			Value = d.ToString();
			DVal = d; LineNumber = line;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
