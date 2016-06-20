using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// An LSN object that is passed by reference;
	/// </summary>
	[Serializable]
	public abstract class LSN_ReferenceValue
	{
		public abstract bool BoolValue { get; }

		private LsnReferenceType _Type;
		public LsnReferenceType Type { get { return _Type; } protected set { _Type = value; } }
	}
}
