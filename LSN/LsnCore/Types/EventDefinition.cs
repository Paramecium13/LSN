using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class EventDefinition
	{
		public readonly string Name;
		public readonly IReadOnlyList<Parameter> Parameters;


		public EventDefinition(string name, IList<Parameter> paramaters)
		{
			if (paramaters.Any(p => !p.DefaultValue.IsNull)) throw new ArgumentException("Event parameters cannot have default values.", "parameters");
			Parameters = paramaters.ToList();
		}

	}
}
