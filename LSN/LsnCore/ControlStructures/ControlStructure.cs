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
			throw new InvalidOperationException();
		}
	}
}
