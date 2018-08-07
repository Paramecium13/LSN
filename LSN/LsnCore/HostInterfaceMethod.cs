/*using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public class HostInterfaceMethod : Method
	{
		public override bool HandlesScope => true;

		public HostInterfaceMethod(TypeId type, TypeId returnType, string name, IList<Parameter> parameters)
			: base(type, returnType, name, parameters) { }

		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
			=> (args[0].Value as IHostInterface).CallMethod(Name, args.Skip(1).ToArray());
	}
}
*/