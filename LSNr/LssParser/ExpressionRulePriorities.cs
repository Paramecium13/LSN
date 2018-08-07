using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.LssParser
{
	public static class ExpressionRulePriorities
	{
		public const uint Constant = 10000;

		public const uint MemberAccess = 7000; // Also used for parenthesis...

		public const uint Function = 6000;

		public const uint Parenthesis = 5000;

		public const uint MultDiv = 4000;

		public const uint AddSub = 3000;

		public const uint Comparative = 2000;
	}
}
