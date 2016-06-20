using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public static class LsnMath
	{

		static LsnMath()
		{
			Sqrt = new BoundedFunction(d =>
			{
				var v = d["x"];
				if (v.GetType() == typeof(IntValue))
				{
					return new DoubleValue(Math.Sqrt(((IntValue)v).Value));
				}
				return new DoubleValue(Math.Sqrt(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) },LsnType.double_);
			
			Sin = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v.GetType() == typeof(IntValue))
				{
					return new DoubleValue(Math.Sin(((IntValue)v).Value));
				}
				return new DoubleValue(Math.Sin(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);

			Cos = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v.GetType() == typeof(IntValue))
				{
					return new DoubleValue(Math.Cos(((IntValue)v).Value));
				}
				return new DoubleValue(Math.Cos(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);

			Tan = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v.GetType() == typeof(IntValue))
				{
					return new DoubleValue(Math.Sin(((IntValue)v).Value));
				}
				return new DoubleValue(Math.Sin(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);
			
		}

		public readonly static Function Sqrt;
		public readonly static Function Sin;
		public readonly static Function Cos;
		public readonly static Function Tan;

	}
}
