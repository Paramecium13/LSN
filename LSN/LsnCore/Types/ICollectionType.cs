using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	/// <summary>
	/// A(n) LSN collection type.
	/// </summary>
	public interface ICollectionType
	{
		/// <summary>
		/// The type of the index.
		/// </summary>
		LsnType IndexType { get; } // TODO: Replace with TypeId?

		/// <summary>
		/// The type of the contents.
		/// </summary>
		LsnType ContentsType { get; } // TODO: Replace with TypeId?
	}
}
