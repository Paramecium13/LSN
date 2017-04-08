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
		internal IReadOnlyList<Component> Components;
		

		public EventListener(EventDefinition definition, LsnEnvironment environment)
		{
			Definition = definition; Environment = environment;
		}


		public void Run(LsnValue[] args, IInterpreter i)
		{
			i.EnterFunctionScope(Environment, StackSize);

			// Assign arguments to the stack.
			for (int argI = 0; argI < args.Length; argI++)
				i.SetValue(argI, args[argI]);

			int n = Components.Count;
			for(int x = 0; x < n; x++)
			{
				var val = Components[x].Interpret(i);
				switch (val)
				{
					case InterpretValue.Base:
						break;
					case InterpretValue.Next:
						throw new ApplicationException("This should not happen.");
					case InterpretValue.Break:
						throw new ApplicationException("I SHALL NOT BE BROKEN!!!\nYou cannot have a break statement directly in an event listener.");
					case InterpretValue.Return: //Exit the function.
						x = Components.Count; // This breaks the for loop.
						break;
					default:
						throw new ApplicationException("This should not happen.");
				}
			}
		}

	}
}
