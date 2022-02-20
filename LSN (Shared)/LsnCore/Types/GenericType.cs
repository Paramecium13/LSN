using System.Collections.Generic;
using System.Text;

namespace LsnCore.Types
{
	/// <summary>
	/// ...
	/// </summary>
	public abstract class GenericType
	{
		protected readonly Dictionary<string, LsnType> Types = new();

		public abstract string Name { get; }

		public virtual bool HasConstNumberOfTypeParams => true;

		public virtual int NumberOfTypeParams => 1;

		protected string GetGenericName(TypeId[] types)
		{
			var s = new StringBuilder(Name);
			if (!HasConstNumberOfTypeParams)
			{
				s.Append('`');
				s.Append(types.Length);
			}
			foreach (var type in types)
			{
				s.Append('`');
				s.Append(type.Name);
			}
			return s.ToString();
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public virtual LsnType GetType(TypeId[] types)
		{
			var name = GetGenericName(types);
			if (Types.TryGetValue(name, out var type))
				return type;
			type = CreateType(types);
			if (Types.ContainsKey(name)) return Types[name];
			Types.Add(name, type);
			return type;
		}

		protected abstract LsnType CreateType(TypeId[] types);
	}
}
