using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	/// <summary>
	/// Loads lsn resources and unique script objects.
	/// </summary>
	public interface IResourceManager
	{
		/// <summary>
		/// Get the unique script object that has the provided name, typically from the current save file...
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		ScriptObject GetUniqueScriptObject(string name);

		/// <summary>
		/// Get the lsn resource that has the provided path. It may be a 'special' resource,
		/// such as a standard library component, where the path does not map to a file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		LsnResourceThing GetResource(string path);
	}

	public abstract class ResourceManager : IResourceManager
	{
		private LsnResourceThing LsnMath;

		/// <summary>
		/// Get the unique script object that has the provided name, typically from the current save file...
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public abstract ScriptObject GetUniqueScriptObject(string name);

		/// <summary>
		/// Get a resource that is not part of the standard library.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected abstract LsnResourceThing GetResourceFromFile(string path);

		/// <summary>
		/// Get the lsn resource that has the provided path. It may be a 'special' resource,
		/// such as a standard library component, where the path does not map to a file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public LsnResourceThing GetResource(string path)
		{
			if (path.StartsWith(@"Lsn Core\", StringComparison.Ordinal))
				path = new string(path.Skip(9).ToArray());
			else if (path.StartsWith(@"std\", StringComparison.Ordinal))
				path = new string(path.Skip(4).ToArray());
			else
				return GetResourceFromFile(path);

			switch (path)
			{
				case "Math":
					if(LsnMath == null)
						LsnMath = LoadMath();
					return LsnMath;
				case "Regex":
				case "RegEx":
					throw new NotImplementedException();
				default:
					throw new ApplicationException("Standard file not found.");
			}
		}

		/// <summary>
		/// Get a resurce from the standard library.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static LsnResourceThing GetStandardLibraryResource(string path)
		{
			switch (path)
			{
				case "Math":
					return LoadMath();
				case "Regex":
				case "RegEx":
					throw new NotImplementedException();
				default:
					throw new ApplicationException();
			}
		}

		/// <summary>
		/// Load the standard library math functions.
		/// </summary>
		/// <returns></returns>
		public static LsnResourceThing LoadMath()
		{
			var functions = new List<Function>
			{
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Sqrt(v.DoubleValue));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Sqrt"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Sin(v.DoubleValue));
				}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Sin"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Cos(v.DoubleValue));
				}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Cos"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Tan(v.DoubleValue));
				}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Tan"),
				new BoundedFunction(d =>
				{
					var x = d[0].DoubleValue;
					var y = d[1].DoubleValue;
					return new LsnValue(Math.Sqrt(x * x + y * y));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0),
				new Parameter("y", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Hypot"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Asin(v.DoubleValue));
				}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ASin"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Acos(v.DoubleValue));
				}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ACos"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Atan(v.DoubleValue));
				}, new List<Parameter> { new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ATan"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Sinh(v.DoubleValue));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Sinh"),
				new BoundedFunction(d =>
				{
					var v = d[0];

					return new LsnValue(Math.Cosh(v.DoubleValue));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Cosh"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Tanh(v.DoubleValue));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Tanh"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Log(v.DoubleValue));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Log"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Math.Log10(v.DoubleValue));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Log10"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return (new LsnValue(Erf(v.DoubleValue)));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ErrorFunction"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return (new LsnValue(Γ(v.DoubleValue)));
				}, new List<Parameter>() { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Gamma")
			};

			return new LsnResourceThing(new TypeId[0])
			{
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				StructTypes = new Dictionary<string, StructType>(),
				ScriptObjectTypes = new Dictionary<string, ScriptObjectType>(),
				//Types = new List<LsnType>(),
				Usings = new List<string>(),
				Functions = functions.ToDictionary((f) => f.Name)
			};
		}

		/// <summary>
		/// The golden ratio
		/// </summary>
		public readonly static double φ = (1 + Math.Sqrt(5)) / 2;

		public readonly static double γ = 0.57721566490153286060651209008240243104215933593992;

		public readonly static double Sqrt2Pi = Math.Sqrt(2 * Math.PI);

		/// <summary>
		/// The square root of 2
		/// </summary>
		public readonly static double Sqrt2 = Math.Sqrt(2);

		private static readonly double[] p = { 676.5203681218851, -1259.1392167224028, 771.32342877765313,
			-176.61502916214059, 12.507343278686905, -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 };

		/// <summary>
		/// The error function.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
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
		/// For a random variable in the standard normal distribution, y, this returns the probability that y greater than x.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double Q_Function(double x)
			=> 0.5 * (1 - Erf(x / Sqrt2));

		/// <summary>
		/// The gamma function
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
