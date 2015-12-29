using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	public class VectorType : LSN_Type
	{
		public override bool BoolVal { get { return true; } }

		private LSN_Type _Generic;
		public LSN_Type GenericType { get { return _Generic; } private set { _Generic = value; } }

		public VectorType(LSN_Type type)
		{
			GenericType = type;
		}




		public override ILSN_Value CreateDefaultValue()
		{
			throw new NotImplementedException();
		}

		public override int GetSize()
		{
			throw new NotImplementedException();
		}
	}
}
