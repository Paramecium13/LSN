using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
{
	public struct IntToken : IToken
	{
		public string Value { get; private set; }
		public int LineNumber { get; }
		public readonly int IVal;

		public IntToken(string s, int line = 0)
		{
			Value = s; LineNumber = line;
			IVal = int.Parse(s);
		}

		public IntToken(int i, int line = 0)
		{
			Value = i.ToString(); LineNumber = line;
			IVal = i;
		}

		public bool Equals(IToken other)
		{
			return TokenExtensions.Equals(this, other);
		}

	}
}
