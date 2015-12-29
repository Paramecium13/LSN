using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// Wraps a .NET reference to an LSN_Object
	/// </summary>
	public class LSN_Reference : LSN_Value
	{
		public readonly LSN_ReferenceValue Value;

		public LSN_Reference(LSN_ReferenceValue value, LSN_ReferenceType type)
		{
			Value = value;
			Type = type;
		}

		public override ILSN_Value Clone() => new LSN_Reference(Value, (LSN_ReferenceType) Type);

		public override int GetSize()
		{
			throw new NotImplementedException();
		}

		public override string TranslateUniversal()
		{
			throw new NotImplementedException();
		}
	}
}
