using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core

{
	public static class ExtensionMethods
	{

		public static List<T> Substitute<T>(this List<T> IEnum, T toReplace, T replacement)
			where T : IEquatable<T>
		{
			List<T> NewIEnum = new List<T>(IEnum.Count());
			foreach (var item in IEnum)
			{
				NewIEnum.Add(item.Equals(toReplace) ? replacement : item);
			}
			return NewIEnum;
		}

		/// <summary>
		/// Converts a list of key-value pairs into a dictionary, no parameters needed.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="self"></param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> self)
			=> self.ToDictionary(pair => pair.Key, pair => pair.Value);
	}
}
