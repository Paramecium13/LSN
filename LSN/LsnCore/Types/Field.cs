﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public struct Field : IEquatable<Field>
	{
		public readonly int Index;
		public readonly string Name;
		public readonly TypeId Type;

		public Field(int index, string name, LsnType type)
		{
			Index = index; Name = name; Type = type.Id;
		}

		public bool Equals(Field other)
			=> Index == other.Index && Name == other.Name && Type == other.Type;
	}
}
