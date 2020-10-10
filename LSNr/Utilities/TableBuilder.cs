using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Utilities
{
	/// <summary>
	/// Builds an ordered table (i.e. array) of unique constants.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public sealed class TableBuilder<TValue> where TValue : IEquatable<TValue>
	{
		/// <summary>
		/// The table being constructed.
		/// </summary>
		private readonly List<TValue> Table = new List<TValue>();

		/// <summary>
		/// A map of entries to their indexes in <see cref="Table"/>.
		/// </summary>
		private readonly Dictionary<TValue, int> IndexLookup = new Dictionary<TValue, int>();

		/// <summary>
		/// Adds the specified <paramref name="value"/> to the table being constructed if it isn't already in the table.
		/// Returns the index of <paramref name="value"/> in the table.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns> The index of <paramref name="value"/> in the table. </returns>
		public int Add(TValue value)
		{
			if (IndexLookup.TryGetValue(value, out var index))
			{
				return index;
			}

			index = Table.Count;
			Table.Add(value);
			return index;
		}

		/// <summary>
		/// Gets the table.
		/// </summary>
		public TValue[] GetTable() => Table.ToArray();
	}
}
