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
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);

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


			Hypot = new BoundedFunction(d =>
			{
				var x = (double)((DoubleValue)d["x"]);
				var y = (double)((DoubleValue)d["y"]);
				return (DoubleValue)Math.Sqrt(x * x + y * y);
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0), new Parameter("y", LsnType.double_, null, 0) }, LsnType.double_);


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
				if (v is IntValue)
					return (new DoubleValue(Math.Log(((IntValue)v).Value)));
				return (new DoubleValue(Math.Log(((DoubleValue)v).Value)));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);

			Log10 = new BoundedFunction(d =>
			{
				var v = d["x"];
				if (v is IntValue)
					return (new DoubleValue(Math.Log10(((IntValue)v).Value)));
				return (new DoubleValue(Math.Log10(((DoubleValue)v).Value)));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);



			ErrorFunction = new BoundedFunction(d =>
			{
				var v = d["x"];
				double x;
				if (v is IntValue)
					x = ((IntValue)v).Value;
				else x = ((DoubleValue)v).Value;
				return (new DoubleValue(Erf(x)));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);

			Gamma = new BoundedFunction(d =>
			{
				var v = d["x"];
				double x;
				if (v is IntValue)
					x = ((IntValue)v).Value;
				else x = ((DoubleValue)v).Value;
				return (new DoubleValue(Γ(x)));
			}, new List<Parameter>() { new Parameter("x", LsnType.double_, null, 0) }, LsnType.double_);

		}

		public readonly static Function Sqrt;

		public readonly static Function Sin;
		public readonly static Function Cos;
		public readonly static Function Tan;

		public readonly static Function Hypot;

		public readonly static Function ASin;
		public readonly static Function ACos;
		public readonly static Function ATan;

		public readonly static Function Sinh;
		public readonly static Function Cosh;
		public readonly static Function Tanh;

		public readonly static Function Log;
		public readonly static Function Log10;

		/// <summary>
		/// The error function.
		/// </summary>
		public readonly static Function ErrorFunction;


		public readonly static Function Gamma;

		public readonly static double φ = (1 + Math.Sqrt(5)) / 2;


		public readonly static double γ = 0.57721566490153286060651209008240243104215933593992;

		public readonly static double Sqrt2Pi = Math.Sqrt(2 * Math.PI);

		private static readonly double[] p = new double[] { 676.5203681218851, -1259.1392167224028, 771.32342877765313,
			-176.61502916214059, 12.507343278686905, -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 };

		public static double Erf(double x)
		{
			var t = 1 / (1 + 0.5 * Math.Abs(x));
			var t2 = t * t;
			var t3 = t2 * t;
			var t4 = t2 * t2;
			var t5 = t3 * t2;
			var t6 = t3 * t3;
			var t7 = t3 * t4;
			var t8 = t4 * t4;
			var t9 = t5 * t4;
			var τ = Math.Exp(-x * x - 1.26551223 + 1.00002368 * t + 0.37409196 * t2 + 0.09678418 * t3 - 0.18628806 * t4 + 0.27886807 * t5
				- 1.13520398 * t6 + 1.48851587 * t7 - 0.82215223 * t8 + 0.17087277 * t9);
			if (x < 0) return τ - 1;
			return 1 - τ;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double Γ(double x)
		{
			if (x < 0.5) return Math.PI / (Math.Sin(Math.PI * x) * Γ(1 - x));
			x -= 1;
			var y = 0.99999999999980993;
			for (int i = 0; i < 8; i++)
				y += p[i] / (x + i + 1);
			var t = x + 8 - 0.5;
			return Sqrt2Pi * Math.Pow(t, x + 0.5) * Math.Exp(-t) * y;
		}

	}
}
