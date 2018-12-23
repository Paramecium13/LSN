using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore.Values
{
	public class RangeValue : ILsnValue
	{
		public readonly int Start;
		public readonly int End;

		public TypeId Type => Types.RangeType.Instance.Id;

		public bool BoolValue => true;

		public RangeValue(int start, int end)
		{
			Start = start; End = end;
		}

		public ILsnValue Clone() => this;

		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write((byte)ConstantCode.Range);
			writer.Write(Start);
			writer.Write(End);
		}
	}
}
