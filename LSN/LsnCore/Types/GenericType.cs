using System.Collections.Generic;
using System.Text;

namespace LsnCore.Types
{
	/// <summary>
	/// ...
	/// </summary>
	public abstract class GenericType
	{
		protected readonly Dictionary<string, LsnType> Types = new Dictionary<string, LsnType>();

		public abstract string Name { get; }

		protected string GetGenericName(TypeId[] types)
		{
			var s = new StringBuilder(Name);
			foreach(var type in types)
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
			LsnType type = null;
			if (Types.TryGetValue(name,out type))
				return type;
			type = CreateType(types);
			if (!Types.ContainsKey(name)) // For some reason this double check is needed to avoid adding duplicate keys.
			{
				Types.Add(name, type);
				return type;
			}
			return Types[name];
		}

		protected abstract LsnType CreateType(TypeId[] types);
	}
}
