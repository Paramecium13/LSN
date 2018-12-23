using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	public sealed class LsnCollectionEnumerator : ILsnEnumerator
	{
		int currentIndex = -1;
		int length;
		readonly ICollectionValue Collection;
		public LsnValue Current { get; private set; } = LsnValue.Nil;

		public LsnCollectionEnumerator(ICollectionValue collection)
		{
			Collection = collection;
		}

		public bool MoveNext()
		{
			if(++currentIndex >= length) return false;
			Current = Collection.GetValue(currentIndex);
			return true;
		}

		/*public void Reset()
		{
			currentIndex = 0;
		}*/
	}

	/// <summary>
	/// A(n) LSN collection value. It contains values that can be read.
	/// </summary>
	public interface ICollectionValue : ILsnEnumerable
	{
		//ICollectionType CollectionType { get; }

		LsnValue GetValue(int index);

		int GetLength();
	}
}
