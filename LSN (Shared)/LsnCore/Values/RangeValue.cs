using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore.Values
{
	public class RangeEnumerator : ILsnEnumerator
	{
		RangeValue Range;
		int current;
		public LsnValue Current => new LsnValue(current);

		public RangeEnumerator(RangeValue range)
		{
			Range = range;
			current = range.Start - 1;
		}

		public bool MoveNext() => ++current <= Range.End;
	}

	/// <summary>
	/// Represents a range of integers.
	/// </summary>
	public class RangeValue : ILsnValue, ILsnEnumerable, IHasFieldsValue
	{
		public readonly int Start;
		public readonly int End;

		public TypeId Type => RangeType.Instance.Id;

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

		public ILsnEnumerator GetLsnEnumerator() => new RangeEnumerator(this);

		public LsnValue GetFieldValue(int index)
		{
			switch (index)
			{
				case 0: return new LsnValue(Start);
				case 1: return new LsnValue(End);
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
