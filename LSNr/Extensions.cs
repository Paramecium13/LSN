using LsnCore;
using Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public static class Extensions
	{
		/// <summary>
		/// Does this have the given token value?
		/// </summary>
		/// <param name="self"></param>
		/// <param name="value"> The value to look for. If ignore case is true, this should be all lowercase.</param>
		/// <param name="ignoreCase"> Should case be ignored. Default is true.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
		public static bool HasToken(this IEnumerable<IToken> self, string value, bool ignoreCase = true)
			=> ignoreCase ?
				self.Any(t => t.Value.ToLower() == value)
				: self.Any(t => t.Value == value);

		/// <summary>
		/// Gets the index of the first token with the provided value, returns -1 if it cannot be found.
		/// </summary>
		/// <param name="self"></param>
		/// <param name="value"> The value to look for. If ignore case is true, this should be all lowercase.</param>
		/// <param name="ignoreCase"> Should case be ignored. Default is true.</param>
		/// <returns></returns>
		public static int IndexOf(this List<IToken> self, string value, bool ignoreCase = true)
		{
			if(ignoreCase)
			{
				for(int i = 0; i < self.Count; i++)
				{
					if (self[i].Value.ToLower() == value) return i;
				}
				return -1;
			}
			for (int i = 0; i < self.Count; i++)
			{
				if (self[i].Value == value) return i;
			}
			return -1;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IList<Tuple<TKey, TValue>> ls)
			=> ls.ToDictionary(x => x.Item1, x => x.Item2);
	}
}
