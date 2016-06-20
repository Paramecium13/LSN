using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// Wraps a .NET reference to an LSN_Object
	/// </summary>
	[Serializable]
	public class LSN_Reference : LSN_Value
	{
		public readonly LSN_ReferenceValue Value;
		public override bool BoolValue { get { return Value?.BoolValue ?? false; } }

		public LSN_Reference(LSN_ReferenceValue value, LsnReferenceType type)
		{
			Value = value;
			Type = type;
		}

		public override ILsnValue Clone() => new LSN_Reference(Value, (LsnReferenceType) Type);

	}
}
