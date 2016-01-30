using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
{
	public struct Identifier : IToken
	{
		public string Value { get; set; }

		public Identifier(string s)
		{
			Value = s;
		}

		public bool IsVariable()
		{
			return false;
		}

		public bool IsFunction()
		{
			return false;
		}

		public int Count()
		{
			return -1;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
