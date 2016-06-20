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
				if (v is IntValue)
					return new DoubleValue(Math.Sqrt(((IntValue)v).Value));
				return new DoubleValue(Math.Sqrt(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) },LsnType.double_);
			
			// Triginometric Functions

			Sin = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v is IntValue)
					return new DoubleValue(Math.Sin(((IntValue)v).Value));
				return new DoubleValue(Math.Sin(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);

			Cos = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v is IntValue)
					return new DoubleValue(Math.Cos(((IntValue)v).Value));
				return new DoubleValue(Math.Cos(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);

			Tan = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v is IntValue)
					return new DoubleValue(Math.Tan(((IntValue)v).Value));
				return new DoubleValue(Math.Tan(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);

			// Inverse Trigonometric Functions

			ASin = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v is IntValue)
					return new DoubleValue(Math.Sin(((IntValue)v).Value));
				return new DoubleValue(Math.Asin(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);

			ACos = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v is IntValue)
					return new DoubleValue(Math.Acos(((IntValue)v).Value));
				return new DoubleValue(Math.Acos(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);

			ATan = new BoundedFunction(d =>
			{
				var v = d["θ"];
				if (v is IntValue)
					return new DoubleValue(Math.Atan(((IntValue)v).Value));
				return new DoubleValue(Math.Atan(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("θ", LsnType.double_, null, 0) }, LsnType.double_);

			// Hyperbolic Functions

			Sinh = new BoundedFunction(d =>
			{
				var v = d["x"];
				if (v is IntValue)
					return new DoubleValue(Math.Sinh(((IntValue)v).Value));
				return new DoubleValue(Math.Sinh(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);

			Cosh = new BoundedFunction(d =>
			{
				var v = d["x"];
				if (v is IntValue)
					return new DoubleValue(Math.Cosh(((IntValue)v).Value));
				return new DoubleValue(Math.Cosh(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);

			Tanh = new BoundedFunction(d =>
			{
				var v = d["x"];
				if (v is IntValue)
					return new DoubleValue(Math.Tanh(((IntValue)v).Value));
				return new DoubleValue(Math.Tanh(((DoubleValue)v).Value));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);


			Log = new BoundedFunction(d =>
			{
				var v = d["x"];
				return (new DoubleValue(Math.Log(((DoubleValue)v).Value)));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);


		}

		public readonly static Function Sqrt;

		public readonly static Function Sin;
		public readonly static Function Cos;
		public readonly static Function Tan;

		public readonly static Function ASin;
		public readonly static Function ACos;
		public readonly static Function ATan;

		public readonly static Function Sinh;
		public readonly static Function Cosh;
		public readonly static Function Tanh;

		public readonly static Function Log;
		public readonly static Function Log10;

	}
}
