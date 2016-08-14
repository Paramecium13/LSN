using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
{
	public struct Keyword : IToken
	{
		public string Value { get; set; }
		public int LineNumber { get; }

		public Keyword(string t, int line = 0)
		{
			Value = t; LineNumber = line;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
