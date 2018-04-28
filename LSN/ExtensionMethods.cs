using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore

{
	public static class ExtensionMethods
	{
		internal static void Write(this BinaryDataWriter self, StatementCode code)
		{
			self.Write((ushort)code);
		}

		public static IEnumerable<T> Substitute<T>(this IEnumerable<T> self, T toReplace, T replacement)
			where T : IEquatable<T>
			=> self.Select(x => x.Equals(toReplace) ? replacement : x);

		/// <summary>
		/// Converts a list of key-value pairs into a dictionary, no parameters needed.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="self"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> self)
			=> self.ToDictionary(pair => pair.Key, pair => pair.Value);

		public unsafe static int ToInt32Bitwise(this double self)
			=> *((int*)&self);
	}
}
