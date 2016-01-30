using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.ControlStructures
{
	[Serializable]
	public abstract class ControlStructure : Component
	{
		protected virtual InterpretValue Interpret(List<Component> components, IInterpreter i)
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
