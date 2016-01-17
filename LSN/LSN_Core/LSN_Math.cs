using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	public static class LSN_Math
	{

		static LSN_Math()
		{
			Sqrt = new BoundedFunction(d =>
			{
				var v = d["x"];
				if (v.GetType() == typeof(IntValue))
				{
					return new DoubleValue(Math.Sqrt(((IntValue)v).Value));
				}
				return new DoubleValue(Math.Sqrt(((DoubleValue)v).Value));
			});
			Sqrt.Parameters = new List<Parameter>();
			Sqrt.Parameters.Add(new Parameter("x", LSN_Type.double_, null, 0));
			Sqrt.ReturnType = LSN_Type.double_;

			Sin = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v.GetType() == typeof(IntValue))
				{
					return new DoubleValue(Math.Sin(((IntValue)v).Value));
				}
				return new DoubleValue(Math.Sin(((DoubleValue)v).Value));
			});
			Sin.Parameters = new List<Parameter>();
			Sin.Parameters.Add(new Parameter("θ", LSN_Type.double_, null, 0));
			Sin.ReturnType = LSN_Type.double_;

			Cos = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v.GetType() == typeof(IntValue))
				{
					return new DoubleValue(Math.Cos(((IntValue)v).Value));
				}
				return new DoubleValue(Math.Cos(((DoubleValue)v).Value));
			});
			Cos.Parameters = new List<Parameter>();
			Cos.Parameters.Add(new Parameter("θ", LSN_Type.double_, null, 0));
			Cos.ReturnType = LSN_Type.double_;

			Tan = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v.GetType() == typeof(IntValue))
				{
					return new DoubleValue(Math.Sin(((IntValue)v).Value));
				}
				return new DoubleValue(Math.Sin(((DoubleValue)v).Value));
			});
			Tan.Parameters = new List<Parameter>();
			Tan.Parameters.Add(new Parameter("θ", LSN_Type.double_, null, 0));
			Tan.ReturnType = LSN_Type.double_;
		}

		public readonly static Function Sqrt;
		public readonly static Function Sin;
		public readonly static Function Cos;
		public readonly static Function Tan;

	}
}
