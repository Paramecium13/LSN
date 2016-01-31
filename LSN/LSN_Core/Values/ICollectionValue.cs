using LSN_Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Values
{
	/// <summary>
	/// A(n) LSN collection value. It's contains values that can be read.
	/// </summary>
	public interface ICollectionValue
	{
		ICollectionType CollectionType { get; }

		ILSN_Value GetValue(ILSN_Value index);
	}
}
