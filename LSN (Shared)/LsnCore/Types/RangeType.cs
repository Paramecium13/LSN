using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Types
{
	public class RangeType : LsnType
	{
		public static readonly RangeType Instance = new RangeType(); 
		public override LsnValue CreateDefaultValue() => new LsnValue(new Values.RangeValue(0, 0));

		private RangeType()
		{
			Name = "IntRange";
			Id = new TypeId(this);
		}
	}
}
