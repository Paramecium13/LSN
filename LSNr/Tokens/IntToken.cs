using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
{
	public struct IntToken : IToken
	{
		public string Value { get; set; }
		public int IVal;

		public IntToken(string s)
		{
			Value = s;
			IVal = int.Parse(s);
		}

		public IntToken(int i)
		{
			Value = i.ToString();
			IVal = i;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
