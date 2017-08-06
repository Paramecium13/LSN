using LsnCore.Statements;
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
		public readonly EventDefinition Definition;

		private readonly LsnEnvironment Environment;

		internal int StackSize;
		internal Statement[] Code;
		

		public EventListener(EventDefinition definition, LsnEnvironment environment)
		{
			Definition = definition; Environment = environment;
		}


		public void Run(LsnValue[] args, IInterpreter i)
		{
			i.Run(Code, Environment, StackSize, args);
		}

	}
}
