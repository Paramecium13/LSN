using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	/*public static class LsnMath
	{
		
		public readonly static Function Sqrt 
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Sqrt(v.DoubleValue));
			}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Sqrt");

		public readonly static Function Sin 
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Sin(v.DoubleValue));
			}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Sin");

		public readonly static Function Cos 
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Cos(v.DoubleValue));
			}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Cos");

		public readonly static Function Tan 
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Tan(v.DoubleValue));
			}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Tan");


		public readonly static Function Hypot
			= new BoundedFunction(d =>
			{
				var x = d[0].DoubleValue;
				var y = d[1].DoubleValue;
				return new LsnValue(Math.Sqrt(x * x + y * y));
			}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0),
				new Parameter("y", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Hypot");

		public readonly static Function ASin 
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Asin(v.DoubleValue));
			}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ASin");

		public readonly static Function ACos 
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Acos(v.DoubleValue));
			}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ACos");

		public readonly static Function ATan 
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Atan(v.DoubleValue));
			}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ATan");


		public readonly static Function Sinh
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Sinh(v.DoubleValue));
			}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Sinh");

		public readonly static Function Cosh
			= new BoundedFunction(d =>
			{
				var v = d[0];

				return new LsnValue(Math.Cosh(v.DoubleValue));
			}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Cosh");

		public readonly static Function Tanh
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Tanh(v.DoubleValue));
			}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Tanh");


		public readonly static Function Log
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Log(v.DoubleValue));
			}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Log");

		public readonly static Function Log10
			= new BoundedFunction(d =>
			{
				var v = d[0];
				return new LsnValue(Math.Log10(v.DoubleValue));
			}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Log10");

		/// <summary>
		/// The error function.
		/// </summary>
		public readonly static Function ErrorFunction = new BoundedFunction(d =>
		{
			var v = d[0];
			return (new LsnValue(Erf(v.DoubleValue)));
		}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ErrorFunction");



		public readonly static Function Gamma = new BoundedFunction(d =>
		{
			var v = d[0];
			return (new LsnValue(Γ(v.DoubleValue)));
		}, new List<Parameter>() { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Gamma");

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

	}*/
}
