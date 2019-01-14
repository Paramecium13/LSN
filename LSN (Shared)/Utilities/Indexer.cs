using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Utilities
{
	public class Indexer<T>
	{
		public class RestorePoint
		{
			int val;
			Indexer<T> Indexer;
			public RestorePoint(int v, Indexer<T> i) { val = v; Indexer = i; }
			public void Restore() { Indexer.Index = val; }
		}

		int Index { get; set; }
		readonly IReadOnlyList<T> Collection;

		public T Current => Collection[Index];

		public Indexer(int index, IReadOnlyList<T> collection)
		{
			Index = index; Collection = collection;
		}

		public RestorePoint CreateRestorePoint() => new RestorePoint(Index, this);

		public Indexer<T> Clone() => new Indexer<T>(Index, Collection);

		/*
		 * CreateBookmark()...
		 *
		 * // Make a slice from Bookmark to current index...
		 * SliceToBookmark()
		 */
		public int LengthAhead => Collection.Count - Index;

		public int LengthBehind => Index;

		public T PeekAhead(int by = 1)
			=> Collection[Index + by];

		public T PeekBehind(int by = 1)
			=> Collection[Index - by];

		public bool TestAhead(Predicate<T> test = null, int by = 1)
		{
			if (Index + by >= Collection.Count) return false;
			if (test == null) return true;
			return test(Collection[Index + by]);
		}

		public bool MoveForward()
		{
			if (Index + 1 >= Collection.Count) return false;
			Index++;
			return true;
		}

		public Slice<T> SliceWhile(Predicate<T> pred, out bool reachedEnd)
		{
			var start = Index;
			reachedEnd = false;
			if (pred == null)
				throw new ArgumentNullException(nameof(pred));
			while (pred(Current))
			{
				if(!MoveForward())
				{
					reachedEnd = true; Index++; break;
				}
			}
			return Slice<T>.Create(Collection, start, Index - start);
		}
	}
}
