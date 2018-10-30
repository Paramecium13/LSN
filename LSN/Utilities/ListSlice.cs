using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Utilities
{
	abstract class ListSliceBase<T,TList> : Slice<T>
		where TList : IList<T>
	{
		public override T this[int index]
		{
			get
			{
				if (index >= Count || index < 0)
					throw new IndexOutOfRangeException();
				return m_list[Start + index];
			}
		}

		protected readonly TList m_list;

		protected ListSliceBase(TList list, int start, int length) : base(start, length)
		{
			if (start + length > list.Count)
				throw new ArgumentOutOfRangeException(nameof(length));
			m_list = list;
		}

		public override IEnumerator<T> GetEnumerator()
		{
			for (int i = Start; i < Start + Count; i++)
			{
				yield return m_list[i];
			}
		}
	}

	class ListSlice<T> : ListSliceBase<T,List<T>>
	{
		public ListSlice(List<T> list, int start, int length) : base(list, start, length) {}

		public override int IndexOf(T value)
			=> m_list.FindIndex(Start, Count, ((T x) => x.Equals(value)));

		public override int IndexOf(T value, int start, int count)
			=> m_list.FindIndex(Start + start, Math.Min(count, Count), ((T x) => x.Equals(value)));

		public override ISlice<T> CreateSubSlice(int start, int count)
		{
			if(start + count >= Count)
				throw new ArgumentOutOfRangeException();
			return new ListSlice<T>(m_list, Start + start, count);
		}
	}

	class ListSliceI<T> : ListSliceBase<T, IList<T>>
	{
		public ListSliceI(IList<T> list, int start, int length) : base(list, start, length) {}

		public override int IndexOf(T value)
		{
			for(int i = Start; i < Start + Count; i++)
			{
				if (m_list[i].Equals(value))
					return i - Start;
			}
			return -1;
		}

		public override int IndexOf(T value, int start, int count)
		{
			for (int i = Start + start; i < Start + Math.Min(count, Count); i++)
			{
				if (m_list[i].Equals(value))
					return i - Start;
			}
			return -1;
		}

		public override ISlice<T> CreateSubSlice(int start, int count)
		{
			if(start + count >= Count)
				throw new ArgumentOutOfRangeException();
			return new ListSliceI<T>(m_list, Start + start, count);
		}
	}

	class ListSliceIR<T> : Slice<T>
	{
		public override T this[int index]
		{
			get
			{
				if (index >= Count || index < 0)
					throw new IndexOutOfRangeException();
				return m_list[Start + index];
			}
		}

		private readonly IReadOnlyList<T> m_list;

		public ListSliceIR(IReadOnlyList<T> list, int start, int length) : base(start, length)
		{
			if (start + length > list.Count)
				throw new ArgumentOutOfRangeException(nameof(length));
			m_list = list;
		}

		public override IEnumerator<T> GetEnumerator()
		{
			for (int i = Start; i < Start + Count; i++)
			{
				yield return m_list[i];
			}
		}

		public override int IndexOf(T value)
		{
			for (int i = Start; i < Start + Count; i++)
			{
				if (m_list[i].Equals(value))
					return i - Start;
			}
			return -1;
		}

		public override int IndexOf(T value, int start, int count)
		{
			for (int i = Start + start; i < Start + Math.Min(count, Count); i++)
			{
				if (m_list[i].Equals(value))
					return i - Start;
			}
			return -1;
		}

		public override ISlice<T> CreateSubSlice(int start, int count)
		{
			if(start + count >= Count)
				throw new ArgumentOutOfRangeException();
			return new ListSliceIR<T>(m_list, Start + start, count);
		}
	}
}
