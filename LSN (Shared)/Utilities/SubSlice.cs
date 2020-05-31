using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Utilities
{
	internal class SubSlice<T> : Slice<T>
	{
		public override T this[int index]
		{
			get
			{
				if (index >= Count || index < 0)
					throw new IndexOutOfRangeException();
				return Slice[Start + index];
			}
		}

		protected readonly ISearchableReadOnlyList<T> Slice;

		public SubSlice(ISearchableReadOnlyList<T> slice, int start, int count):base(start,count)
		{
			if (start + count > slice.Count)
				throw new ArgumentOutOfRangeException(nameof(count));
			Slice = slice;
		}

		public override IEnumerator<T> GetEnumerator()
		{
			for (var i = Start; i < Start + Count; i++)
				yield return Slice[i];
		}

		public override int IndexOf(T value)
			=> Slice.IndexOf(value, Start, Count);

		public override int IndexOf(T value, int start, int count)
			=> Slice.IndexOf(value, Start + start, Math.Min(Count, count));

		public override ISlice<T> CreateSubSlice(int start, int count)
		{
			if(start + count > Count)
				throw new ArgumentOutOfRangeException();
			return new SubSlice<T>(Slice, Start + start, count);
		}
	}
}