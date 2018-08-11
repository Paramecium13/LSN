using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.LssParser
{
	public static class ExpressionRulePriorities
	{
		public const uint Constant		= 10000;

		public const uint MemberAccess	= 8000; // Also used for parenthesis and a bunch of other stuff...

		public const uint Function		= 7000;

		public const uint Parenthesis	= 6000;

		public const uint Negation		= 5500;

		public const uint Exponents		= 5000;

		public const uint MultDiv		= 4000;

		public const uint AddSub		= 3000;

		public const uint Comparative	= 2000;

		public const uint Logical		= 1000;
	}
}
