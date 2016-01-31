using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Types
{
	/// <summary>
	/// A(n) LSN collection type.
	/// </summary>
	public interface ICollectionType
	{
		/// <summary>
		/// The type of the index.
		/// </summary>
		LSN_Type IndexType { get; }

		/// <summary>
		/// The type of the contents.
		/// </summary>
		LSN_Type ContentsType { get; }
	}
}
