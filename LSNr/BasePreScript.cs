using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using Tokens;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LSNr
{
	public abstract class BasePreScript : IPreScript
	{

		/// <summary>
		/// 
		/// </summary>
		protected readonly string Source;


		protected string Text;


		private bool _Mutable = false;


		protected IReadOnlyList<IToken> Tokens;


		public abstract IScope CurrentScope { get; set; }

		/// <summary>
		/// Are variables defined in this script mutable?
		/// </summary>
		public bool Mutable { get { return _Mutable; } protected set { _Mutable = value; } }

		/// <summary>
		/// Is this script valid (free of errors)?
		/// </summary>
		public bool Valid { get; set; } = true;


		public BasePreScript(string src)
		{
			Source = src;
			IncludeFunction(LsnMath.ACos);
			IncludeFunction(LsnMath.ASin);
			IncludeFunction(LsnMath.ATan);
			IncludeFunction(LsnMath.Cos);
			IncludeFunction(LsnMath.Cosh);
			IncludeFunction(LsnMath.ErrorFunction);
			IncludeFunction(LsnMath.Gamma);
			IncludeFunction(LsnMath.Hypot);
			IncludeFunction(LsnMath.Log);
			IncludeFunction(LsnMath.Log10);
			IncludeFunction(LsnMath.Sin);
			IncludeFunction(LsnMath.Sinh);
			IncludeFunction(LsnMath.Sqrt);
			IncludeFunction(LsnMath.Tan);
			IncludeFunction(LsnMath.Tanh);
		}

		/// <summary>
		/// 
		/// </summary>
		protected void Tokenize()
		{
			//Tokens = Tokenizer.Tokenize(Text);
			var tokenizer = new CharStreamTokenizer();
			Tokens = tokenizer.Tokenize(Text);
		}

		protected void Include(LsnResourceThing resource)
		{
			foreach(var pair in resource.Functions)
			{
				if (IncludedFunctions.ContainsKey(pair.Key)) throw new ApplicationException();
				IncludedFunctions.Add(pair.Key, pair.Value);
			}
			// ToDo: Add types.

		}

		protected LsnResourceThing Load(string path)
		{
			LsnResourceThing res;
			using (var fs = new FileStream(path, FileMode.Open))
			{
				res = (LsnResourceThing)(new BinaryFormatter().Deserialize(fs));
			}
			return res;
		}

		protected void Use(LsnResourceThing resource, string path)
		{
			foreach (var pair in resource.Functions)
			{
				if (LoadedExternallyDefinedFunctions.ContainsKey(pair.Key)) throw new ApplicationException();
				LoadedExternallyDefinedFunctions.Add(pair.Key, pair.Value);
			}
			// ToDo: Add types.
			Usings.Add(path);
		}		

		#region Functions

		/// <summary>
		/// The functions included in this script.
		/// </summary>
		protected readonly Dictionary<string, Function> IncludedFunctions = new Dictionary<string, Function>();

		/// <summary>
		/// The external functions that to be linked at runtime.
		/// </summary>
		protected readonly Dictionary<string, Function> ExternalFunctions = new Dictionary<string, Function>();

		/// <summary>
		/// 
		/// </summary>
		private Dictionary<string, Function> LoadedExternallyDefinedFunctions = new Dictionary<string, Function>();

		protected List<string> Usings = new List<string>();

		public bool FunctionExists(string name)
			=> IncludedFunctions.ContainsKey(name) || LoadedExternallyDefinedFunctions.ContainsKey(name);

		public bool FunctionIsIncluded(string name)
			=> IncludedFunctions.ContainsKey(name);

		public Function GetFunction(string name)
		{
			if (IncludedFunctions.ContainsKey(name)) return IncludedFunctions[name];
			if (LoadedExternallyDefinedFunctions.ContainsKey(name)) return AddExternalFunction(LoadedExternallyDefinedFunctions[name]);
			else throw new ApplicationException($"Function \"{name}\" not found. Are you missing an include or using?");
		}

		/// <summary>
		/// Adds a function defined in another file.
		/// </summary>
		/// <param name="fn"> The fully created and functional externally defined function.</param>
		/// <returns></returns>
		private ExternalFunction AddExternalFunction(Function fn)
		{
			var exFn = new ExternalFunction(fn.Name, fn.Parameters.Select(p => new Parameter(p.Name, p.Type, p.DefaultValue, p.Index)).ToList(),
				fn.StackSize,fn.ReturnType);
			ExternalFunctions.Add(fn.Name, exFn);
			IncludedFunctions.Add(fn.Name, exFn);
			return exFn;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fn"></param>
		protected void IncludeFunction(Function fn)
		{
			IncludedFunctions.Add(fn.Name, fn);
		}

		/// <summary>
		/// Load a function. If it is used, include it...
		/// </summary>
		/// <param name="fn"></param>
		protected void LazyIncludeFunction(Function fn)
		{
			throw new NotImplementedException();
		}


		#endregion

		#region Types

		/// <summary>
		/// The types that will be included by the output.
		/// </summary>
		protected readonly IList<LsnType> IncludedTypes = new List<LsnType>();

		/// <summary>
		/// The types that will not be included by the output.
		/// </summary>
		protected readonly IList<LsnType> LoadedTypes = LsnType.GetBaseTypes();


		protected readonly IList<GenericType> IncludedGenerics = new List<GenericType>();


		protected readonly IList<GenericType> LoadedGenerics = LsnType.GetBaseGenerics();


		public virtual bool TypeExists(string name)
			=> IncludedTypes.Any(t => t.Name == name) || LoadedTypes.Any(t => t.Name == name);


		public virtual LsnType GetType(string name)
		{
			var type = IncludedTypes.FirstOrDefault(t => t.Name == name);
			if (type != null) return type;
			type = LoadedTypes.FirstOrDefault(t => t.Name == name);
			return type;
		}


		public virtual bool GenericTypeExists(string name)
			=> IncludedGenerics.Any(t => t.Name == name) || LoadedGenerics.Any(t => t.Name == name);


		public virtual GenericType GetGenericType(string name)
		{
			var type = IncludedGenerics.FirstOrDefault(t => t.Name == name);
			if (type != null) return type;
			type = LoadedGenerics.FirstOrDefault(t => t.Name == name);
			return type;
		}



		#endregion
	}
}
