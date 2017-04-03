using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class MethodDefinition
	{
		public readonly string Name;
		public readonly IReadOnlyList<Parameter> Parameters;
		public readonly TypeId ReturnType;

		public MethodDefinition(string name, IList<Parameter> parameters, TypeId returnType)
		{
			Name = name; Parameters = parameters?.ToList() ?? new List<Parameter>(0); ReturnType = returnType;
		}
	}
}
