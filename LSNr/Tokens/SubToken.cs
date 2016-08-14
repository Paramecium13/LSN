using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
{
	public struct SubToken : IToken
	{
		public string Value { get; set; }
		public int LineNumber { get; }

		public SubToken(string value, int line = 0)
		{
			Value = value; LineNumber = line;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
