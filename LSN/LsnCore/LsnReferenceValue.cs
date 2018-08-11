using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// An LSN object that is passed by reference;
	/// </summary>
	[Serializable]
	public abstract class LsnReferenceValue
	{
		public abstract bool BoolValue { get; }

		private LsnReferenceType _Type;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public LsnReferenceType Type { get { return _Type; } protected set { _Type = value; } }
	}
}
