using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public struct Field : IEquatable<Field>
	{
		public readonly int Index;
		public readonly string Name;
		public readonly TypeId Type;
		public readonly bool Mutable;

		public Field(int index, string name, LsnType type, bool mutable = false)
		{
			Index = index; Name = name; Type = type.Id; Mutable = mutable;
		}

		public Field(int index, string name, TypeId type, bool mutable = false)
		{
			Index = index; Name = name; Type = type; Mutable = mutable;
		}

		public bool Equals(Field other)
			=> Index == other.Index && Name == other.Name && Type == other.Type;
	}
}
