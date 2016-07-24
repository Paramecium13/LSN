﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens.Tokens
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
			return Value == other.Value;
		}
	}
}
