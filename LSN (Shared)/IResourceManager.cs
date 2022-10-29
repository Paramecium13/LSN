using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Runtime.Types;
using LsnCore.Runtime.Values;
using LsnCore.Interpretation;

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
		LsnObjectFile GetResource(string path);

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
					return LsnMath ??= LoadMath();
				case "Random":
					return LsnRandom ??= LoadRandom();
				case "Regex":
				case "RegEx":
					throw new NotImplementedException();
				case "Read":
					return LsnRead ??= LoadRead();
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
					return LsnMath ??= LoadMath();
				case "Random":
					return LsnRandom ??= LoadRandom();
				case "Read":
					return LsnRead ??= LoadRead();
				case "Regex":
				case "RegEx":
					throw new NotImplementedException();
				default:
					throw new ApplicationException();
			}
		}

#if LSNR
		internal static Function Sqrt { get; private set; }
		internal static Function InvSqrt { get; private set; }
		internal static Function Hypot { get; private set; }
		internal static Function Sin { get; private set; }
		internal static Function Cos { get; private set; }
		internal static Function Tan { get; private set; }
		internal static Function ASin { get; private set; }
		internal static Function ACos { get; private set; }
		internal static Function ATan { get; private set; }

		private static void CreateMathFunctions()
		{
			Sqrt = new InstructionMappedFunction(
				new List<Parameter> {new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0)}, LsnType.double_, "Sqrt",
				OpCode.Sqrt);
			InvSqrt = new InstructionMappedFunction(
				new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Sqrt",
				OpCode.InvSqrt);
			Sin = new InstructionMappedFunction(
				new List<Parameter> {new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0)}, LsnType.double_, "Sin",
				OpCode.Sin);
			Cos = new InstructionMappedFunction(
				new List<Parameter> {new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0)}, LsnType.double_, "Cos",
				OpCode.Cos);
			Tan = new InstructionMappedFunction(
				new List<Parameter> {new Parameter("θ", LsnType.double_.Id, LsnValue.Nil, 0)}, LsnType.double_, "Tan",
				OpCode.Tan);
			Hypot = new MultiInstructionMappedFunction(new List<Parameter>
				{
					new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0),
					new Parameter("y", LsnType.double_.Id, LsnValue.Nil, 0)
				}, LsnType.double_, "Hypot",
				new[]
				{
					// load x
					new (OpCode opCode, ushort data)[] {(OpCode.Dup, 0), (OpCode.Mul, 0)},
					// load y
					new (OpCode opCode, ushort data)[] {(OpCode.Dup, 0), (OpCode.Mul, 0), (OpCode.Sqrt, 0)},
				});
			ASin = new InstructionMappedFunction(
				new List<Parameter> {new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0)}, LsnType.double_, "ASin",
				OpCode.ASin);
			ACos = new InstructionMappedFunction(new List<Parameter> {new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0)},
				LsnType.double_, "ACos", OpCode.ACos);
			ATan = new InstructionMappedFunction(
				new List<Parameter> {new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0)}, LsnType.double_, "ATan",
				OpCode.ATan);
		}
#endif
		/// <summary>
		/// Load the standard library math functions.
		/// </summary>
		/// <returns></returns>
		private static LsnResourceThing LoadMath()
		{
			#if LSNR
			if (Sqrt == null)
			{
				CreateMathFunctions();
			}
			#endif
			var functions = new Function[]
			{
			#if LSNR
				Sqrt,Sin,Cos,Tan,Hypot,ASin,ACos,ATan,
				
				// ToDo: Hyperbolic trig OpCodes?
				new InstructionMappedFunction(new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Sinh", OpCode.HCF),
				new InstructionMappedFunction(new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Cosh", OpCode.HCF),
				new InstructionMappedFunction(new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Tanh", OpCode.HCF),

				new InstructionMappedFunction(new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Log", OpCode.Log),
				new InstructionMappedFunction(new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Log10", OpCode.Log10),
				new InstructionMappedFunction(new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Log2", OpCode.Log2),
				new MultiInstructionMappedFunction(new List<Parameter> { new Parameter("base", LsnType.double_, LsnValue.Nil, 0),
					new Parameter("x", LsnType.double_, LsnValue.Nil, 1)}, LsnType.double_, "LogB", new[]
				{
					new (OpCode opCode, ushort data)[]
					{
						(OpCode.Log, 0)		// , b -> , log(b)
					},
					new (OpCode opCode, ushort data)[]
					{
						(OpCode.Log, 0),	// , log(b), x -> , log(b), log(x)
						(OpCode.Swap, 0),	// , log(b), log(x) -> log(x), log(b)
						(OpCode.Div_F64, 0)
					}
				}),
				new InstructionMappedFunction(new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "ErrorFunction", OpCode.Erf),
				new InstructionMappedFunction(new List<Parameter> { new Parameter("x", LsnType.double_.Id, LsnValue.Nil, 0) }, LsnType.double_, "Gamma", OpCode.Gamma),
				new InstructionMappedFunction(new List<Parameter>
				{
					new Parameter("a", LsnType.double_.Id, LsnValue.Nil, 0) , new Parameter("b", LsnType.double_.Id, LsnValue.Nil, 1)

				}, LsnType.double_, "Max", OpCode.Max),
				new InstructionMappedFunction(new List<Parameter>
				{
					new Parameter("a", LsnType.double_.Id, LsnValue.Nil, 0) , new Parameter("b", LsnType.double_.Id, LsnValue.Nil, 1)

				}, LsnType.double_, "Min", OpCode.Min),
				new InstructionMappedFunction(new List<Parameter>
				{
					new Parameter("a", LsnType.int_.Id, LsnValue.Nil, 0) , new Parameter("b", LsnType.int_.Id, LsnValue.Nil, 1)

				}, LsnType.double_, "MaxI", OpCode.Max),
				new InstructionMappedFunction(new List<Parameter>
				{
					new Parameter("a", LsnType.int_.Id, LsnValue.Nil, 0) , new Parameter("b", LsnType.int_.Id, LsnValue.Nil, 1)

				}, LsnType.double_, "MinI", OpCode.Min),
#endif
			};

			return new LsnResourceThing(new[] { LsnType.int_.Id, LsnType.double_.Id})
			{
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				StructTypes = new Dictionary<string, StructType>(),
				ScriptClassTypes = new Dictionary<string, ScriptClass>(),
				//Types = new List<LsnType>(),
				Usings = new List<string>(),
				Functions = functions.ToDictionary((f) => f.Name)
			};
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static LsnResourceThing LoadRandom()
		{
			var functions = new Function[]
			{
#if LSNR
				new InstructionMappedFunction(new List<Parameter>(), LsnType.double_, "Random", OpCode.Rand),
				new InstructionMappedFunction(new List<Parameter> {new Parameter("seed",LsnType.int_,new LsnValue(0),0)},(TypeId) null, "SetRandomSeed", OpCode.Srand),
				new InstructionMappedFunction(new List<Parameter>
					{
						new Parameter("min",LsnType.int_,new LsnValue(0),0), new Parameter("max",LsnType.int_,new LsnValue(int.MaxValue),1)
					}, LsnType.int_, "RandomInt", OpCode.RandInt),
				new MultiInstructionMappedFunction(new List<Parameter>
					{
						new Parameter("min",LsnType.double_,new LsnValue(0.0),0), new Parameter("max",LsnType.double_,new LsnValue(1.0),1)
					},
					LsnType.double_, "RandomDouble", new (OpCode opCode, ushort data)[][]
					{
						new (OpCode opCode, ushort data)[]
						{
							(OpCode.Swap, 0),	// , min, max -> , max, min
							(OpCode.SwaDup, 0),	// , max, min -> , min, max, min
							(OpCode.Swap, 0),	// , min, max, min -> , min, min, max
							(OpCode.Sub, 0),	// , min, min, max -> , min, (max - min)
							(OpCode.Rand, 0),	// , min, (max - min), -> , min, (max - min), rand
							(OpCode.Mul, 0),	// , min, (max - min), rand -> , min, (max - min) * rand
							(OpCode.Add, 0)		// , min, (max - min) * rand ->  , min + (max - min) * rand
						}
					}),
				// ToDo: Normal rand...
				new InstructionMappedFunction(new List<Parameter>(), LsnType.double_, "StandardNormal", OpCode.HCF),
				new InstructionMappedFunction(new List<Parameter> {new Parameter("cap",LsnType.double_, new LsnValue(4.0),0)},
					LsnType.double_, "CappedStandardNormal", OpCode.HCF),
				new InstructionMappedFunction(
					new List<Parameter> {new Parameter("mean",LsnType.double_,new LsnValue(0.0),0), new Parameter("standardDeviation",LsnType.double_,new LsnValue(1.0),1)},
					LsnType.double_, "Normal", OpCode.HCF),
				new InstructionMappedFunction(
					new List<Parameter> { new Parameter("mean", LsnType.double_, new LsnValue(0.0), 0),
						new Parameter("standardDeviation", LsnType.double_, new LsnValue(1.0), 1),
						new Parameter("cap",LsnType.double_, new LsnValue(4.0),2)},
					LsnType.double_, "CappedNormal", OpCode.HCF),

				new MultiInstructionMappedFunction(new List<Parameter> {new Parameter("percent",LsnType.double_,new LsnValue(50.0),0)},
					LsnType.double_, "PercentChance", new[]
					{
						new (OpCode opCode, ushort data)[]
						{
							(OpCode.LoadConst_I32_short, 100),	// , percent -> , percent, 100
							(OpCode.Swap, 0),					// , percent, 100 -> , 100, percent
							(OpCode.Div_F64, 0),				// , 100, percent -> , percent/100
							(OpCode.Rand, 0),					// , percent/100 -> , percent/100, rand
							(OpCode.Lt, 0),						// , percent/100, rand -> , rand < percent/100
						}
					}),
				new MultiInstructionMappedFunction(new List<Parameter> {new Parameter("percent",LsnType.double_,new LsnValue(50.0),0)},
					LsnType.double_, "PercentChanceB", new[]
					{
						new (OpCode opCode, ushort data)[]
						{
							(OpCode.Rand, 0),					// , percent -> , percent, rand
							(OpCode.LoadConst_I32_short, 100),	// , percent, rand -> , percent, rand, 100
							(OpCode.Mul, 0),					// , percent, rand, 100 -> , percent, rand * 100
							(OpCode.Lt, 0),						// , percent, rand * 100 -> , rand * 100 < percent
						}
					}),
#endif
			};
			return new LsnResourceThing(Array.Empty<TypeId>() /*{ LsnType.int_.Id, LsnType.double_.Id }*/)
			{
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				StructTypes = new Dictionary<string, StructType>(),
				ScriptClassTypes = new Dictionary<string, ScriptClass>(),
				//Types = new List<LsnType>(),
				Usings = new List<string>(),
				Functions = functions.ToDictionary((f) => f.Name)
			};

		}

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
		private static LsnResourceThing LoadRead()
		{
			var prompt = new Parameter("prompt", LsnType.string_, LsnValue.Nil, 0);
			var functions = new Function[]
			{
				#if LSNR
				new InstructionMappedFunction(new List<Parameter>{prompt}, LsnType.int_.Id, "GetInt", OpCode.ReadInt),
				new InstructionMappedFunction(new List<Parameter>{prompt}, LsnType.string_.Id,"GetString", OpCode.ReadString),
				new InstructionMappedFunction(new List<Parameter>{prompt}, LsnType.double_.Id, "GetDouble", OpCode.ReadDouble)
				#endif
			};

			return new LsnResourceThing(Array.Empty<TypeId>())
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
