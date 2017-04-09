using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class EventDefinition
	{
		public readonly string Name;
		public readonly IReadOnlyList<Parameter> Parameters;


		public EventDefinition(string name, IList<Parameter> paramaters)
		{
			if (paramaters.Any(p => !p.DefaultValue.IsNull)) throw new ArgumentException("Event parameters cannot have default values.", "parameters");
			Parameters = paramaters.ToList();
			Name = name;
		}

		public bool Equivalent(EventDefinition other, bool requireMatchingNames = false)
		{
			if (Name != other.Name) return false;
			if (Parameters.Count != other.Parameters.Count) return false;

			for(int i = 0; i < Parameters.Count; i++)
			{
				var param = Parameters[i];
				var otherParam = other.Parameters[i];
				if (param.Type != otherParam.Type) return false;
				if (param.Index != otherParam.Index) return false;
				if (requireMatchingNames && param.Name != otherParam.Name) return false;
			}

			return true;
		}
	}
}
