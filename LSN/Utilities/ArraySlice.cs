using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Utilities
{
	public class ArraySlice<T> : Slice<T>
	{
		public override T this[int index]
		{
			get
			{
				if (index >= Count || index < 0)
					throw new IndexOutOfRangeException();
				return m_array[Start + index];
			}
		}

		private readonly T[] m_array;

		public ArraySlice(T[] array, int start, int length) : base(start, length)
		{
			if (start + length > array.Length)
				throw new ArgumentOutOfRangeException(nameof(length));
			m_array = array;
		}

		public override IEnumerator<T> GetEnumerator()
		{
			for(int i = Start; i < Start + Count; i++)
			{
				yield return m_array[i];
			}
		}

		public override int IndexOf(T value)
			=> Array.FindIndex(m_array, Start, Count, ((T x) => x.Equals(value)));

		public override int IndexOf(T value, int start, int count)
			=> Array.FindIndex(m_array, Start + start, Math.Min(count, Count), ((T x) => x.Equals(value)));

		public override ISlice<T> CreateSubSlice(int start, int count)
		{
			if(start + count >= Count)
				throw new ArgumentOutOfRangeException();
			return new ArraySlice<T>(m_array, Start + start, count);
		}
	}
}
