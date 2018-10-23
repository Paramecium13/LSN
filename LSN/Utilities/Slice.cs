using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Utilities
{
	public abstract class Slice<T> : ISearchableReadOnlyList<T>
	{
		public abstract T this[int index] { get; }

		public int Count { get; }

		protected readonly int Start;

		protected Slice(int start, int length)
		{
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			if (length <= 0)
				throw new ArgumentOutOfRangeException(nameof(length));
			Count = length;
			Start = start;
		}

		public abstract IEnumerator<T> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public abstract int IndexOf(T value);

		public abstract int IndexOf(T value, int start, int count);

		public static Slice<T> Create(T[] array, int start, int count)
			=> new ArraySlice<T>(array, start, count);

		public static Slice<T> Create(List<T> list, int start, int count)
			=> new ListSlice<T>(list, start, count);

		public static Slice<T> Create(IList<T> list, int start, int count)
			=> new ListSliceI<T>(list, start, count);

		public static Slice<T> Create(IReadOnlyList<T> list, int start, int count)
			=> new ListSliceIR<T>(list, start, count);

		public static Slice<T> Create(ISearchableReadOnlyList<T> slice, int start, int count)
			=> new SubSlice<T>(slice, start, count);

		public static Slice<T> Create(ArraySegment<T> segment)
			=> new ArraySlice<T>(segment.Array, segment.Offset, segment.Count);
	}
}
