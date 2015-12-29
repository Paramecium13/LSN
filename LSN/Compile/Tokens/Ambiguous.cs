using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Compile.Tokens
{
	public struct Ambiguous : IToken
	{
		public string Value { get; set; }

		public Ambiguous(string s)
		{
			Value = s;
		}

		public bool Equals(IToken other)
		{
			throw new NotImplementedException();
		}
	}
}
