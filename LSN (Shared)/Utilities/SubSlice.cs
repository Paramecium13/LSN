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
				return m_slice[Start + index];
			}
		}

		protected readonly ISearchableReadOnlyList<T> m_slice;

		public SubSlice(ISearchableReadOnlyList<T> slice, int start, int count):base(start,count)
		{
			if (start + count > slice.Count)
				throw new ArgumentOutOfRangeException(nameof(count));
			m_slice = slice;
		}

		public override IEnumerator<T> GetEnumerator()
		{
			for (int i = Start; i < Start + Count; i++)
				yield return m_slice[i];
		}

		public override int IndexOf(T value)
			=> m_slice.IndexOf(value, Start, Count);

		public override int IndexOf(T value, int start, int count)
			=> m_slice.IndexOf(value, Start + start, Math.Min(Count, count));

		public override ISlice<T> CreateSubSlice(int start, int count)
		{
			if(start + count > Count)
				throw new ArgumentOutOfRangeException();
			return new SubSlice<T>(m_slice, Start + start, count);
		}
	}
}
