using LSN_Core.Compile;
using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.ControlStructures
{
	public abstract class ControlStructure : Component
	{
		internal List<string> HeaderTokens;
		internal List<string> BodyTokens;
		
		public Scope _Scope { get; protected set; }

	}
}
