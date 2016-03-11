using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Types
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public abstract class GenericType
	{

		internal readonly static VectorGeneric Vector = new VectorGeneric();

		private readonly Dictionary<string, LSN_Type> Types = new Dictionary<string, LSN_Type>();

		public abstract string Name { get; }

		private string GetGenericName(List<LSN_Type> types)
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
		public LSN_Type GetType(List<LSN_Type> types)
		{
			var name = GetGenericName(types);
			if (Types.ContainsKey(name)) return Types[name];
			var type = CreateType(types);
			Types.Add(name, type);
			return type;
		}

		protected abstract LSN_Type CreateType(List<LSN_Type> types);

	}
}
