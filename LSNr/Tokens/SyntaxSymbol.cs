using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
{
	public struct SyntaxSymbol : IToken
	{
		public string Value { get; set; }

		public SyntaxSymbol(string s)
		{
			Value = s;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this,other);
		}

	}
}
