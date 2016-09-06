using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.ControlStructures
{
	[Serializable]
	public abstract class ControlStructure : Component
	{
		// TODO: Add a Components Getter so the components can be optimized.

		protected virtual InterpretValue Interpret(IReadOnlyList<Component> components, IInterpreter i)
		{
			for(int j = 0; j < components.Count; j++)
			{
				var val = components[j].Interpret(i);
				switch (val)
				{
					case InterpretValue.Base:
						break;
					case InterpretValue.Next:
						return InterpretValue.Next;
					case InterpretValue.Break:
						return InterpretValue.Break;
					case InterpretValue.Return:
						return InterpretValue.Return;
					default:
						break;
				}
			}

			return InterpretValue.Base;
		}
	}
}
