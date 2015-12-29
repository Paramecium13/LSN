using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Compile.Tokens
{
	public struct Operator : IToken
	{
		public string Value { get; set; }

		public Operator(string t)
		{
			Value = t;
		}


		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
