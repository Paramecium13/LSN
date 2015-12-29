using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Compile.Tokens
{
	public struct Keyword : IToken
	{
		public string Value { get; set; }
		public Keyword(string t)
		{
			Value = t;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
