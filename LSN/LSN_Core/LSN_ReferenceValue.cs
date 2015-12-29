using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// An LSN object that is passed by reference;
	/// </summary>
	public abstract class LSN_ReferenceValue
	{
		private LSN_ReferenceType _Type;
		public LSN_ReferenceType Type { get { return _Type; } protected set { _Type = value; } }
	}
}
