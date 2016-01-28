using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSN_Core.Expressions;

namespace LSN_Core
{
	/// <summary>
	/// A method written in LSN
	/// </summary>
	public class LSN_Method : Method
	{
		public override bool HandlesScope { get { return false; } }



		public override ILSN_Value Eval(Dictionary<string, ILSN_Value> args, IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
}
