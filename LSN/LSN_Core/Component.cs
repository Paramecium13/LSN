using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	/// <summary>
	/// The basis LSN class, children include control structures and statements.
	/// </summary>
	[Serializable]
	public abstract class Component
	{
		/// <summary>
		/// The return value is false when the script, function or loop should stop (e.g. break or return.).
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public abstract bool Interpret(IInterpreter i);
	}
}
