using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Values
{
	public interface ILsnEnumerator
	{
		LsnValue Current { get; }
		bool MoveNext();
		//void Reset();
	}

	public interface ILsnEnumerable
	{
		ILsnEnumerator GetLsnEnumerator();
	}
}
