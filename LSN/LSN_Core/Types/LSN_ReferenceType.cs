using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	public abstract class LSN_ReferenceType : LSN_Type
	{
		public override bool BoolVal { get { return true; } }
	}
}
