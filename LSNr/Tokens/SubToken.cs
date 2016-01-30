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

		public SubToken(string value)
		{
			Value = value;
			//Value = Preprocessor.Subs[s];
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
