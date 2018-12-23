using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Utilities
{
	public interface ISearchableReadOnlyList<T> : IReadOnlyList<T>
	{
		int IndexOf(T value);

		int IndexOf(T value, int start, int count);
	}

	public interface ISlice<T> :  ISearchableReadOnlyList<T>
	{
		int Length { get; }

		ISlice<T> CreateSubSlice(int start, int count);
	}

	public static class SliceExtensions
	{
		public static ISlice<T> CreateSliceAt<T>(this ISlice<T> self, int index)
			=> self.CreateSubSlice(index, self.Count - index);

		public static ISlice<T> CreateSliceTaking<T>(this ISlice<T> self, int count)
			=> self.CreateSubSlice(0, count);

		public static ISlice<T> CreateSliceSkipTake<T>(this ISlice<T> self, int skip, int take)
			=> self.CreateSubSlice(skip, take);

		public static ISlice<T> CreateSliceBetween<T>(this ISlice<T> self, int index1, int index2)
			=> self.CreateSubSlice(index1, index2 - index1);

		public static Slice<T> ToSlice<T>(this List<T> self)
			=> new ListSlice<T>(self, 0, self.Count);

		public static Slice<T> ToSlice<T>(this IReadOnlyList<T> self)
			=> new ListSliceIR<T>(self, 0, self.Count);

		public static Slice<T> ToSlice<T>(this T[] self)
			=> new ArraySlice<T>(self, 0, self.Length);

		public static Slice<T> ToSlice<T>(this ArraySegment<T> segment)
			=> new ArraySlice<T>(segment.Array, segment.Offset, segment.Count);
	}
}
