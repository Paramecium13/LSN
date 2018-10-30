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
		ISlice<T> CreateSubSlice(int start, int count);
	}
}
