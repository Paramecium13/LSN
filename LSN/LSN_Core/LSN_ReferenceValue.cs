using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// An LSN object that is passed by reference;
	/// </summary>
	[Serializable]
	public abstract class LSN_ReferenceValue
	{
		public abstract bool BoolValue { get; }

		private LSN_ReferenceType _Type;
		public LSN_ReferenceType Type { get { return _Type; } protected set { _Type = value; } }
	}
}
