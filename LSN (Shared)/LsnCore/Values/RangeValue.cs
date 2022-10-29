using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore.Values
{
	public class RangeEnumerator : ILsnEnumerator
	{
		private readonly RangeValue Range;
		private int current;
		public LsnValue Current => new LsnValue(current);

		public RangeEnumerator(RangeValue range)
		{
			Range = range;
			current = range.Start - 1;
		}

		public bool MoveNext() => ++current <= Range.End;
	}

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

		public void Serialize(BinaryStream writer)
		{
			writer.Write((byte)ConstantCode.Range);
			writer.Write(Start);
			writer.Write(End);
		}

		public ILsnEnumerator GetLsnEnumerator() => new RangeEnumerator(this);

		public LsnValue GetFieldValue(int index)
		{
			return index switch
			{
				0 => new LsnValue(Start),
				1 => new LsnValue(End),
				_ => throw new InvalidOperationException()
			};
		}
	}
}
