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
		//ToDo: Add a field for its file...
		public readonly string Name;

		[NonSerialized]
		protected LsnType _Type;

		public LsnType Type => _Type;

		public TypeId(LsnType t)
		{
			Name = t.Name; _Type = t;
		}

		internal TypeId(string name)
		{
			Name = name;
		}

		public void Load(LsnType t)
		{
			_Type = t;
		}

		public override bool Equals(object obj)	
			=> Equals(obj as TypeId);
		

		public override int GetHashCode()
			=> Name.GetHashCode();

		public override string ToString()
			=> Name;

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

		/// <summary>
		/// Is the other type assignable to a variable of this type?
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool Subsumes(LsnType type)
			=> Type?.Subsumes(type) ?? false;

		/// <summary>
		/// Is the other type assignable to a variable of this type?
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool Subsumes(TypeId type)
			=> Type?.Subsumes(type.Type) ?? false;
	}
}
