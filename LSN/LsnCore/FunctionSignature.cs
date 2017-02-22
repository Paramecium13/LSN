using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	[Serializable]
	public sealed class FunctionSignature
	{
		public readonly IReadOnlyList<Parameter> Parameters;
		public readonly string Name;
		public readonly TypeId ReturnType;

		public FunctionSignature(IList<Parameter> parameters,string name, TypeId returnType)
		{
			Parameters = parameters.ToList(); Name = name; ReturnType = returnType;
		}
	}
}
