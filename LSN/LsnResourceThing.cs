using LsnCore;
using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	/// <summary>
	/// Contains functions, structs, constants, and macros/inlines.
	/// </summary>
	[Serializable]
	public class LsnResourceThing : LsnScriptBase
	{
		private LsnEnvironment Environment = null;

		public LsnEnvironment GetEnvironment()
		{
			if (Environment == null) Environment = new LsnEnvironment(this);
			return Environment;
		}
	}
}
