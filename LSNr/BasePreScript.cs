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
using System.Text.RegularExpressions;

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
		protected IReadOnlyList<Token> Tokens;
		public abstract IScope CurrentScope { get; set; }

		/// <summary>
		/// Are variables defined in this script mutable?
		/// </summary>
		public bool Mutable { get { return _Mutable; } protected set { _Mutable = value; } }

		/// <summary>
		/// Is this script valid (free of errors)?
		/// </summary>
		public bool Valid { get; set; } = true;

		protected readonly List<string> Usings = new List<string>();

		protected readonly List<string> Includes = new List<string>();

		protected LsnEnvironment _Environment;
		internal LsnEnvironment Environment => _Environment;

		protected readonly string Path;

		protected BasePreScript(string src, string path)
		{
			Source = src;
			IncludeFunction(LsnMath.ACos);
			IncludeFunction(LsnMath.ASin);
			IncludeFunction(LsnMath.ATan);
			IncludeFunction(LsnMath.Cos);
			IncludeFunction(LsnMath.Cosh);
			IncludeFunction(LsnMath.ErrorFunction);
			IncludeFunction(LsnMath.Gamma);
			IncludeFunction(LsnMath.Log);
			IncludeFunction(LsnMath.Log10);
			IncludeFunction(LsnMath.Sin);
			IncludeFunction(LsnMath.Sinh);
			IncludeFunction(LsnMath.Tan);
			IncludeFunction(LsnMath.Tanh);
			
			IncludeFunction(LsnMath.Sqrt);
			IncludeFunction(LsnMath.Hypot);
			//ToDo: Make it use, rather than include, math functions.

			foreach (var t in LoadedTypes)
				t.Id.Load(t);// Load/bind it's type ids...
		}

		/// <summary>
		/// 
		/// </summary>
		protected void Tokenize()
		{
			Tokens = new CharStreamTokenizer().Tokenize(Text);
		}

		//
		protected void Include(LsnScriptBase resource, string path)
		{
			foreach (var recType in resource.RecordTypes.Values)
			{
				IncludedTypes.Add(recType);
				recType.Id.Load(recType);
			}
			foreach (var stType in resource.StructTypes.Values)
			{
				IncludedTypes.Add(stType);
				stType.Id.Load(stType);
			}
			foreach (var hostInterface in resource.HostInterfaces.Values)
			{
				IncludedTypes.Add(hostInterface);
				hostInterface.Id.Load(hostInterface);
				LoadType(hostInterface);
			}
			foreach(var scObj in resource.ScriptObjectTypes.Values)
			{
				IncludedTypes.Add(scObj);
				scObj.Id.Load(scObj);
				LoadType(scObj);
			}
			//ToDo: Generic types and functions...
			foreach(var pair in resource.Functions)
			{
				// if (IncludedFunctions.ContainsKey(pair.Key)) throw new ApplicationException();
				if (! IncludedFunctions.ContainsKey(pair.Key)) IncludedFunctions.Add(pair.Key, pair.Value);
				LoadFunctionParamAndReturnTypes(pair.Value);
			}

			Includes.Add(path);
		}

		//
		public abstract SymbolType CheckSymbol(string name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns></returns>
		protected LsnResourceThing Load(string path)
		{
			LsnResourceThing res = null;
			string objPath = Program.GetObjectPath(path);
			string srcPath = Program.GetSourcePath(path);
			if (ObjectFileUpToDate(path,out res))
			{
				if (res == null)
				{
					using (var fs = new FileStream(objPath, FileMode.Open))
					{
						res = (LsnResourceThing)(new BinaryFormatter().Deserialize(fs));
					}
				}
			}
			else
			{
				if (Program.MakeResource(System.IO.Path.GetFileNameWithoutExtension(path),File.ReadAllText(srcPath), objPath, out res, new string[0]) != 0)
					throw new ApplicationException();
			}
			return res;
		}

		protected static bool ObjectFileUpToDate(string path, out LsnResourceThing res)
		{
			res = null;
			bool upToDate = true;
			var objPath = Program.GetObjectPath(path);
			var srcPath = Program.GetSourcePath(path);

			if (File.Exists(objPath))
			{
				if (File.Exists(srcPath))
				{ // Both an object file and a source file exists.
					var objTime = File.GetLastWriteTimeUtc(objPath);
					var srcTime = File.GetLastWriteTimeUtc(srcPath);

					if (srcTime < objTime)
					{ // The object file is up to date.
						using (var fs = new FileStream(objPath, FileMode.Open))
						{
							var bf = new BinaryFormatter();
							var obj = bf.Deserialize(fs);
							res = (LsnResourceThing) obj;
						}
						LsnResourceThing x = null;
						foreach (var include in res.Includes)
						{
							if (!ObjectFileUpToDate(include,out x))
							{
								upToDate = false;
								break;
							}
						}
					}
					else upToDate = false;
				}
			}
			else
				upToDate = false;
			return upToDate;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		protected string ProcessDirectives(string source)
		{
			var usePaths = new List<string> { Path };
			if (Regex.IsMatch(source, "#using", RegexOptions.IgnoreCase))
			{
				//Process #using directive(s)
				foreach (var match in Regex.Matches(source, "#using\\s+\"(.+)\"").Cast<Match>())
				{
					var path = match.Groups.OfType<object>().Select(o => o.ToString()).Skip(1).First();
					var res = Load(path);
					Use(res, path);
					source = source.Replace(match.Value, ""); // TODO: Test.
					usePaths.Add(path);
				}
			}
			_Environment = new LsnEnvironment(usePaths);

			if (Regex.IsMatch(source, "#include", RegexOptions.IgnoreCase))
			{
				//Process #include directive(s)
				foreach (var match in Regex.Matches(source, "#include\\s+\"(.+)\"").Cast<Match>())
				{
					var path = match.Groups.OfType<object>().Select(o => o.ToString()).Skip(1).First();
					var res = Load(path);
					Include(res, path);
					source = source.Replace(match.Value, ""); // TODO: Test.
				}
			}
			if (source.Contains("#mut"))
			{
				Mutable = true;
				source = source.Replace("#mut", "");
			}
			if (source.Contains("#no_std"))
			{
				source = source.Replace("#no_std", "");
			}
			else
			{
				// Load the standard library.
			}
			return source;
		}


		protected void Use(LsnScriptBase resource, string path)
		{
			foreach (var u in resource.Usings)
			{
				var res = Load(u);
				Use(res, u);
			}

			foreach (var recType in resource.RecordTypes.Values)
			{
				LoadedTypes.Add(recType);
				recType.Id.Load(recType);
			}
			foreach (var stType in resource.StructTypes.Values)
			{
				LoadedTypes.Add(stType);
				stType.Id.Load(stType);
			}
			foreach (var hostInterface in resource.HostInterfaces.Values)
			{
				LoadedTypes.Add(hostInterface);
				LoadType(hostInterface);
			}
			foreach (var scObj in resource.ScriptObjectTypes.Values)
			{
				LoadedTypes.Add(scObj);
				scObj.Id.Load(scObj);
				LoadType(scObj);
			}
			foreach (var pair in resource.Functions)
			{
				if (!LoadedExternallyDefinedFunctions.ContainsKey(pair.Key))
				{
					LoadedExternallyDefinedFunctions.Add(pair.Key, pair.Value);
					LoadFunctionParamAndReturnTypes(pair.Value);
				}
			}
			//ToDo: Generic types and functions...
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
		private readonly Dictionary<string, Function> LoadedExternallyDefinedFunctions = new Dictionary<string, Function>();


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

		protected void LoadFunctionParamAndReturnTypes(Function func)
		{
			if (func.ReturnType != null)
				LoadType(GetType(func.ReturnType.Name));
			foreach(var param in func.Parameters)
			{
				LoadType(GetType(param.Type.Name));
			}
		}

		protected void LoadFunctionParamAndReturnTypes(FunctionSignature func)
		{
			if (func.ReturnType != null)
			{
				func.ReturnType.Load(GetType(func.ReturnType.Name));
				LoadType(func.ReturnType.Type);
			}
			foreach (var param in func.Parameters)
			{
				param.Type.Load(GetType(param.Type.Name));
				LoadType(param.Type.Type);
			}
		}

		#region Types


		protected void LoadType(LsnType type)
		{
			if (type.Id.Type == null)
			{
				type.Id.Load(type);
				// Methods...
				foreach (var func in type.Methods.Values)
					LoadFunctionParamAndReturnTypes(func);
				
				var fType = type as IHasFieldsType;
				var hType = type as HostInterfaceType;
				if (fType != null)
				{
					foreach (var field in fType.FieldsB)
						LoadType(GetType(field.Type.Name));
					// ScriptObject: Load host, methods, properties
				}
				// HostInterface: Load method defs (FunctionDefinition), event defs.
				else if (hType != null)
				{
					foreach(var method in hType.MethodDefinitions.Values)
						LoadFunctionParamAndReturnTypes(method);
					foreach(var ev in hType.EventDefinitions.Values)
						foreach (var param in ev.Parameters)
							LoadType(GetType(param.Type.Name));					
				}
			}
		}


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
			if (name.Contains('`'))
			{
				var names = name.Split('`');
				if (GenericTypeExists(names[0]))
				{
					var generic = GetGenericType(names[0]);
					return generic.GetType(names.Skip(1).Select(n => GetType(n)).Select(t => t.Id).ToList());
				}
				else throw new ApplicationException();
			}
			var type = IncludedTypes.FirstOrDefault(t => t.Name == name);
			if (type != null) return type;
			type = LoadedTypes.FirstOrDefault(t => t.Name == name);
			return type;
		}

		public virtual bool UniqueScriptObjectTypeExists(string name)
			=> LoadedTypes.Union(IncludedTypes).Any((t) => t.Name == name && ((t as ScriptObjectType)?.Unique ?? false));

		public virtual bool GenericTypeExists(string name)
			=> IncludedGenerics.Any(t => t.Name == name) || LoadedGenerics.Any(t => t.Name == name);


		public virtual GenericType GetGenericType(string name)
		{
			var type = IncludedGenerics.FirstOrDefault(t => t.Name == name);
			if (type != null) return type;
			type = LoadedGenerics.FirstOrDefault(t => t.Name == name);
			return type;
		}

		public virtual TypeId GetTypeId(string name) => GetType(name).Id;


		public bool TypeIsIncluded(TypeId type)
			=> IncludedTypes.Contains(type.Type);


		/*public virtual void AddType(LsnType type)
		{
			IncludedTypes.Add(type);
		}*/

		#endregion
	}
}
