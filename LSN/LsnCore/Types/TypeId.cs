using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public class TypeId : IEquatable<TypeId>
	{
		public readonly string Name;

		[NonSerialized]
		public readonly LsnType Type;

		public TypeId(LsnType t)
		{
			Name = t.Name; Type = t;
		}

		public override bool Equals(object obj)	
			=> Equals(obj as TypeId);
		

		public override int GetHashCode()
			=> Name.GetHashCode();


		public bool Equals(TypeId other)
			=> Name == other?.Name;
	
			
		public static bool operator ==(TypeId a, TypeId b)	
			=> a?.Name == b?.Name;


		public static bool operator !=(TypeId a, TypeId b)
			=> a?.Name != b?.Name;


		public static bool operator ==(LsnType a, TypeId b)
			=> a?.Name == b?.Name;


		public static bool operator !=(LsnType a, TypeId b)
			=> a?.Name != b?.Name;

		public bool Subsumes(LsnType type)
			=> Type?.Subsumes(type) ?? false;
	}
}
