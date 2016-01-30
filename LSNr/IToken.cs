using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokens
{
	public interface IToken : IEquatable<IToken>
	{
		string Value { get; }
	}

	public static class TokenExtensions
	{

		public static bool Equals(IToken a, IToken b)
		{
			// Putting Value comparison first to improve performance by short circuiting becaues it is
			// most likely to fail and I think it is easier to do.
			return a.Value == b.Value && a.GetType().Equals(b.GetType());
		}

	}

}
