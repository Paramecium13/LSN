using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	public class LSN_List : LSN_ReferenceValue
	{
		private List<ILSN_Value> Values = new List<ILSN_Value>();

		public override bool BoolValue { get { return true; } }

		public ILSN_Value this[int index] { get { return Values[index]; } set { Values[index] = value; } }

		public LSN_List(LSN_ListType type)
		{
			Type = type;
		}

	}
}
