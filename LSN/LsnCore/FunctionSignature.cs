using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	[Serializable]
	public sealed class FunctionSignature : IEquatable<FunctionSignature>
	{
		public readonly string Name;
		public readonly IReadOnlyList<Parameter> Parameters;
		public readonly TypeId ReturnType;

		public FunctionSignature(IList<Parameter> parameters,string name, TypeId returnType)
		{
			Parameters = parameters.ToList(); Name = name; ReturnType = returnType;
		}

		public bool Equals(FunctionSignature other)
		{
			if (Name != other.Name)
				return false;
			if (ReturnType != other.ReturnType)
				return false;
			if (Parameters.Count != other.Parameters.Count)
				return false;
			for (int i = 0; i < Parameters.Count; i++)
				if (!Parameters[i].Equals(other.Parameters[i]))
					return false;
			return true;
		}
	}
}
