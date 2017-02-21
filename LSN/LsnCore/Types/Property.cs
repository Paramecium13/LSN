using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class Property
	{
		public readonly string Name;
		public readonly TypeId Type;

		public readonly LsnValue DefaultValue;
	}
}
