using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class EventListener
	{
		private readonly LsnEnvironment Environment;
		private readonly int StackSize;

		public void Run(LsnValue[] args, IInterpreter i)
		{
			i.EnterFunctionScope(Environment, StackSize);
			for (int argI = 0; argI < args.Length; argI++)
				i.SetValue(argI, args[argI]);
		}

	}
}
