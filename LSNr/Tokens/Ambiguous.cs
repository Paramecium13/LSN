using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
{
	public struct Ambiguous : IToken
	{
		public string Value { get; set; }
		public int LineNumber { get; }

		public Ambiguous(string s, int line = 0)
		{
			Value = s; LineNumber = line;
		}

		public bool Equals(IToken other)
		{
			return Value == other.Value;
		}
	}
}
