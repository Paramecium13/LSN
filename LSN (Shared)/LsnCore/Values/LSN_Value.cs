using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Text;
using Syroot.BinaryData;
using LsnCore.Types;
using System.Collections;

namespace LsnCore
{
	/// <summary>
	/// An object representing an LSN value.
	/// </summary>
	[Serializable]
	public abstract class LsnValueB : ILsnValue
	{
		public /*virtual*/ TypeId Type { get; protected set; }

		/// <summary>
		/// ?
		/// </summary>
		public abstract bool BoolValue { get; }

		public abstract ILsnValue Clone();

		public static bool IsPure => true;

		public static bool IsReifyTimeConst() => true;
		public abstract void Serialize(BinaryDataWriter writer);
	}
}
