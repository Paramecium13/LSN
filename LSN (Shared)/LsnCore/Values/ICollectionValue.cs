using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	/// <summary>
	/// A(n) LSN collection value. It contains values that can be read.
	/// </summary>
	public interface ICollectionValue
	{
		//ICollectionType CollectionType { get; }

		LsnValue GetValue(int index);

		int GetLength();
	}
}
