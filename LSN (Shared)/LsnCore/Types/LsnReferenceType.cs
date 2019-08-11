using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// Instances of this are serialized...
	/// </summary>
	public abstract class LsnReferenceType : LsnType
	{
		internal abstract void WriteValue(ILsnValue value, ILsnSerializer serializer, BinaryDataWriter writer);
	}
}
