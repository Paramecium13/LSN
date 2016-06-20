using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public abstract class GenericType
	{
		private readonly Dictionary<string, LsnType> Types = new Dictionary<string, LsnType>();

		public abstract string Name { get; }

		private string GetGenericName(List<LsnType> types)
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
		/// 
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public LsnType GetType(List<LsnType> types)
		{
			var name = GetGenericName(types);
			if (Types.ContainsKey(name)) return Types[name];
			var type = CreateType(types);
			Types.Add(name, type);
			return type;
		}

		protected abstract LsnType CreateType(List<LsnType> types);

	}
}
