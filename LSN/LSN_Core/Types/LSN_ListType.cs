using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// The type of a generic LSN_List.
	/// </summary>
	[Serializable]
	public class LSN_ListType : LSN_ReferenceType
	{
		private LSN_Type _Generic;
		public LSN_Type GenericType { get { return _Generic; } private set { _Generic = value; } }

		public override ILSN_Value CreateDefaultValue()
		{
			throw new NotImplementedException();
		}

	}
}
