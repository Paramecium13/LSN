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
	/// Loads LSN resources and unique script objects.
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
		/// Get the unique script object that has the provided name, typically from the current save file...
		/// </summary>
		/// <param name="scriptClass"></param>
		/// <returns></returns>
		ScriptObject GetUniqueScriptObject(ScriptClass scriptClass);

		/// <summary>
		/// Get the LSN resource that has the provided path. It may be a 'special' resource,
		/// such as a standard library component, where the path does not map to a file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		LsnResourceThing GetResource(string path);

		LsnValue[] LoadValues(string id);

		void SaveValues(LsnValue[] values, string id);

		LsnType GetLsnType(string typeName);

		// Only the one of these that matches the value in Settings needs to be implemented
		IHostInterface GetHostInterface(uint id);
		IHostInterface GetHostInterface(string id);

		// Only the one of these that matches the values in Settings needs to be implemented
		ScriptObject GetScriptObject(uint scriptId);
		ScriptObject GetScriptObject(string scriptId);
		ScriptObject GetScriptObject(uint hostId, uint scriptId);
		ScriptObject GetScriptObject(uint hostId, string scriptId);
		ScriptObject GetScriptObject(string hostId, uint scriptId);
		ScriptObject GetScriptObject(string hostId, string scriptId);
	}

	public abstract class ResourceManager : IResourceManager
	{
		private static LsnResourceThing LsnMath;
		private static LsnResourceThing LsnRandom;
		private static LsnResourceThing LsnRead;

		/// <summary>
		/// Get the unique script object that has the provided name, typically from the current save file...
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public abstract ScriptObject GetUniqueScriptObject(string name);

		/// <summary>
		/// Get the unique script object of the provided type, typically from the current save file...
		/// </summary>
		/// <param name="scriptClass"></param>
		/// <returns></returns>
		public abstract ScriptObject GetUniqueScriptObject(ScriptClass scriptClass);

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
					return LsnMath ?? (LsnMath = LoadMath());
				case "Random":
					return LsnRandom ?? (LsnRandom = LoadRandom());
				case "Regex":
				case "RegEx":
					throw new NotImplementedException();
				case "Read":
					return LsnRead ?? (LsnRead = LoadRead());
				default:
					throw new ApplicationException("Standard file not found.");
			}
		}

		/// <summary>
		/// Get a resource from the standard library.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static LsnResourceThing GetStandardLibraryResource(string path)
		{
			switch (path)
			{
				case "Math":
					return LsnMath ?? (LsnMath = LoadMath());
				case "Random":
					return LsnRandom ?? (LsnRandom = LoadRandom());
				case "Read":
					return LsnRead ?? (LsnRead = LoadRead());
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static LsnResourceThing LoadMath()
		{
			var functions = new Function[]
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
					return new LsnValue(Erf(v.DoubleValue));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ErrorFunction"),
				new BoundedFunction(d =>
				{
					var v = d[0];
					return new LsnValue(Γ(v.DoubleValue));
				}, new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Gamma")
			};

			return new LsnResourceThing(new TypeId[] { LsnType.int_.Id, LsnType.double_.Id})
			{
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				StructTypes = new Dictionary<string, StructType>(),
				ScriptClassTypes = new Dictionary<string, ScriptClass>(),
				//Types = new List<LsnType>(),
				Usings = new List<string>(),
				Functions = functions.ToDictionary((f) => f.Name)
			};
		}
#if CORE
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static LsnResourceThing LoadRandom()
		{
			var functions = new Function[]
			{
				new BoundedFunctionWithInterpreter((i,_) => new LsnValue(i.RngGetDouble()), new List<Parameter>(), LsnType.double_.Id, "Random"),
				new BoundedFunctionWithInterpreter((i,d) =>
				{
					i.RngSetSeed(d[0].IntValue);
					return LsnValue.Nil;
				},new List<Parameter> {new Parameter("seed",LsnType.int_,new LsnValue(0),0)},null, "SetRandomSeed"),
				new BoundedFunctionWithInterpreter((i,d) => new LsnValue(i.RngGetInt(d[0].IntValue,d[1].IntValue)),
					new List<Parameter> {new Parameter("min",LsnType.int_,new LsnValue(0),0), new Parameter("max",LsnType.int_,new LsnValue(int.MaxValue),1)},
					LsnType.int_.Id, "RandomInt"),
				new BoundedFunctionWithInterpreter((i,d) => new LsnValue(i.RngGetDouble(d[0].DoubleValue, d[1].DoubleValue)),
					new List<Parameter> {new Parameter("min",LsnType.double_,new LsnValue(0.0),0), new Parameter("max",LsnType.double_,new LsnValue(1.0),1)},
					LsnType.double_.Id, "RandomDouble"),
				new BoundedFunctionWithInterpreter((i,d) => new LsnValue(i.RngGetNormal()),
					new List<Parameter>(), LsnType.double_.Id, "StandardNormal"),
				new BoundedFunctionWithInterpreter((i,d) => new LsnValue(i.RngGetCappedNormal(d[0].DoubleValue)),
					new List<Parameter> {new Parameter("cap",LsnType.double_, new LsnValue(4.0),0)},
					LsnType.double_.Id, "CappedStandardNormal"),
				new BoundedFunctionWithInterpreter((i,d) => new LsnValue(i.RngGetNormal(d[0].DoubleValue, d[1].DoubleValue)),
					new List<Parameter> {new Parameter("mean",LsnType.double_,new LsnValue(0.0),0), new Parameter("standardDeviation",LsnType.double_,new LsnValue(1.0),1)},
					LsnType.double_.Id, "Normal"),
				new BoundedFunctionWithInterpreter((i,d) => new LsnValue(i.RngGetCappedNormal(d[0].DoubleValue, d[1].DoubleValue,d[2].DoubleValue)),
					new List<Parameter> { new Parameter("mean", LsnType.double_, new LsnValue(0.0), 0),
						new Parameter("standardDeviation", LsnType.double_, new LsnValue(1.0), 1),
						new Parameter("cap",LsnType.double_, new LsnValue(4.0),2)},
					LsnType.double_.Id, "CappedNormal"),

				new BoundedFunctionWithInterpreter((i,d) => new LsnValue(i.RngGetDouble() < d[0].DoubleValue),
					new List<Parameter> {new Parameter("percent",LsnType.double_,new LsnValue(50.0),0)},
					LsnType.double_.Id, "PercentChance"),


			};
			return new LsnResourceThing(new TypeId[0] /*{ LsnType.int_.Id, LsnType.double_.Id }*/)
			{
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				StructTypes = new Dictionary<string, StructType>(),
				ScriptClassTypes = new Dictionary<string, ScriptClass>(),
				//Types = new List<LsnType>(),
				Usings = new List<string>(),
				Functions = functions.ToDictionary((f) => f.Name)
			};

		}
#else
		private static LsnResourceThing LoadRandom()
		{
			var functions = new Function[]
			{
				new BoundedFunctionWithInterpreter(null, new List<Parameter>(), LsnType.double_.Id, "Random"),
				new BoundedFunctionWithInterpreter(null,new List<Parameter> {new Parameter("seed",LsnType.int_,new LsnValue(0),0)},null, "SetRandomSeed"),
				new BoundedFunctionWithInterpreter(null,
					new List<Parameter> {new Parameter("min",LsnType.int_,new LsnValue(0),0), new Parameter("max",LsnType.int_,new LsnValue(int.MaxValue),1)},
					LsnType.int_.Id, "RandomInt"),
				new BoundedFunctionWithInterpreter(null,
					new List<Parameter> {new Parameter("min",LsnType.double_,new LsnValue(0.0),0), new Parameter("max",LsnType.double_,new LsnValue(1.0),1)},
					LsnType.double_.Id, "RandomDouble"),
				new BoundedFunctionWithInterpreter(null,
					new List<Parameter>(), LsnType.double_.Id, "StandardNormal"),
				new BoundedFunctionWithInterpreter(null,
					new List<Parameter> {new Parameter("cap",LsnType.double_, new LsnValue(4.0),0)},
					LsnType.double_.Id, "CappedStandardNormal"),
				new BoundedFunctionWithInterpreter(null,
					new List<Parameter> {new Parameter("mean",LsnType.double_,new LsnValue(0.0),0), new Parameter("standardDeviation",LsnType.double_,new LsnValue(1.0),1)},
					LsnType.double_.Id, "Normal"),
				new BoundedFunctionWithInterpreter(null,
					new List<Parameter> { new Parameter("mean", LsnType.double_, new LsnValue(0.0), 0),
						new Parameter("standardDeviation", LsnType.double_, new LsnValue(1.0), 1),
						new Parameter("cap",LsnType.double_, new LsnValue(4.0),2)},
					LsnType.double_.Id, "CappedNormal"),

				new BoundedFunctionWithInterpreter(null,
					new List<Parameter> {new Parameter("percent",LsnType.double_,new LsnValue(50.0),0)},
					LsnType.double_.Id, "PercentChance"),


			};
			return new LsnResourceThing(new TypeId[0] /*{ LsnType.int_.Id, LsnType.double_.Id }*/)
			{
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				StructTypes = new Dictionary<string, StructType>(),
				ScriptClassTypes = new Dictionary<string, ScriptClass>(),
				//Types = new List<LsnType>(),
				Usings = new List<string>(),
				Functions = functions.ToDictionary((f) => f.Name)
			};

		}
#endif
		/// <summary>
		/// The golden ratio
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "φ")]
		public static readonly double φ = (1 + Math.Sqrt(5)) / 2;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "γ")]
		public static readonly double γ = 0.57721566490153286060651209008240243104215933593992;

		public static readonly double Sqrt2Pi = Math.Sqrt(2 * Math.PI);

		/// <summary>
		/// The square root of 2
		/// </summary>
		public static readonly double Sqrt2 = Math.Sqrt(2);

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
			for (var i = 0; i < 8; i++)
				y += p[i] / (x + i + 1);
			var t = x + 8 - 0.5;
			return Sqrt2Pi * Math.Pow(t, x + 0.5) * Math.Exp(-t) * y;
		}
#if CORE
		private static LsnResourceThing LoadRead()
		{
			var prompt = new Parameter("prompt", LsnType.string_, LsnValue.Nil, 0);
			var functions = new Function[]
			{
				new BoundedFunctionWithInterpreter((i,v)=>new LsnValue(i.GetInt((v[0].Value as StringValue).Value)),
					new List<Parameter>{prompt}, LsnType.int_.Id, "GetInt"),
				new BoundedFunctionWithInterpreter((i,v)=>new LsnValue(new StringValue(i.GetString((v[0].Value as StringValue).Value))),
					new List<Parameter>{prompt}, LsnType.string_.Id,"GetString"),
				new BoundedFunctionWithInterpreter((i,v)=>new LsnValue(i.GetDouble((v[0].Value as StringValue).Value)),
					new List<Parameter>{prompt}, LsnType.double_.Id, "GetDouble")
			};

			return new LsnResourceThing(new TypeId[0])
			{
				Functions = functions.ToDictionary(f => f.Name),
				GameValues = new Dictionary<string, GameValue>(),
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				RecordTypes = new Dictionary<string, RecordType>(),
				ScriptClassTypes = new Dictionary<string, ScriptClass>(),
				StructTypes = new Dictionary<string, StructType>(),
				Usings = new List<string>()
			};
		}
#else
		private static LsnResourceThing LoadRead()
		{
			var prompt = new Parameter("prompt", LsnType.string_, LsnValue.Nil, 0);
			var functions = new Function[]
			{
				new BoundedFunctionWithInterpreter(null, new List<Parameter>{prompt}, LsnType.int_.Id, "GetInt"),
				new BoundedFunctionWithInterpreter(null, new List<Parameter>{prompt}, LsnType.string_.Id, "GetString"),
				new BoundedFunctionWithInterpreter(null, new List<Parameter>{prompt}, LsnType.double_.Id, "GetDouble")
			};

			return new LsnResourceThing(new TypeId[0])
			{
				Functions = functions.ToDictionary(f => f.Name),
				GameValues = new Dictionary<string, GameValue>(),
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				RecordTypes = new Dictionary<string, RecordType>(),
				ScriptClassTypes = new Dictionary<string, ScriptClass>(),
				StructTypes = new Dictionary<string, StructType>(),
				Usings = new List<string>()
			};
		}

#endif
		public abstract LsnValue[] LoadValues(string id);

		public abstract void SaveValues(LsnValue[] values, string id);

		public abstract LsnType GetLsnType(string typeName);

		public abstract IHostInterface GetHostInterface(uint id);
		public abstract IHostInterface GetHostInterface(string id);

		public abstract ScriptObject GetScriptObject(uint scriptId);
		public abstract ScriptObject GetScriptObject(string scriptId);

		public abstract ScriptObject GetScriptObject(uint hostId, uint scriptId);
		public abstract ScriptObject GetScriptObject(uint hostId, string scriptId);
		public abstract ScriptObject GetScriptObject(string hostId, uint scriptId);
		public abstract ScriptObject GetScriptObject(string hostId, string scriptId);
	}
}
