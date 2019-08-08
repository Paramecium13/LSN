using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	/// <summary>
	/// A(n) LSN collection value whose values can also be written.
	/// </summary>
	public interface IMutableCollectionValue : ICollectionValue
	{

		void SetValue(int index, LsnValue value);

	}
}
