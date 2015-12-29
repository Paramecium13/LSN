using LSN_Core;
using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	/// <summary>
	/// Contains functions, structs, constants, and macros/inlines.
	/// </summary>
	[Serializable]
	public class LSN_ResourceThing : LSN_ScriptBase
	{
		private LSN_Environment Environment = null;

		public LSN_Environment GetEnvironment()
		{
			if (Environment == null) Environment = new LSN_Environment(this);
			return Environment;
		}
	}
}
