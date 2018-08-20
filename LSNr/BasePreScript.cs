using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace LSNr
{
	public abstract class BasePreScript : IPreScript
	{
		/// <summary>
		/// ...
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

		protected readonly Dictionary<string, GameValue> LoadedGameValues = new Dictionary<string, GameValue>();

		//protected LsnEnvironment _Environment;
		//internal LsnEnvironment Environment => _Environment;

		public string Path { get; private set; }

		protected BasePreScript(string src, string path)
		{
			Source = src;
			Path = path;
			foreach (var t in LoadedTypes)
				t.Id.Load(t);// Load/bind it's type ids...
		}

		/// <summary>
		/// ...
		/// </summary>
		protected void Tokenize()
		{
			Tokens = new CharStreamTokenizer().Tokenize(Text);
		}

		//
		public abstract SymbolType CheckSymbol(string name);

		private static readonly Func<string,LsnResourceThing> ResourceLoader =
			(path)	=>
			{
				if (path.StartsWith(@"Lsn Core\", StringComparison.Ordinal))
					return ResourceManager.GetStandardLibraryResource(new string(path.Skip(9).ToArray()));
				if (path.StartsWith(@"std\", StringComparison.Ordinal))
					return ResourceManager.GetStandardLibraryResource(new string(path.Skip(4).ToArray()));
				var objPath = Program.GetObjectPath(path);
				using (var stream = File.OpenRead(objPath))
				{
					return LsnResourceThing.Read(stream, new string(objPath.Skip(4).Reverse().Skip(4).Reverse().ToArray()), ResourceLoader);
				}
			};

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns></returns>
		protected static LsnResourceThing Load(string path)
		{
			LsnResourceThing res = null;
			var objPath = Program.GetObjectPath(path);
			var srcPath = Program.GetSourcePath(path);
			if (ObjectFileUpToDate(path,out res))
			{
				if (res == null)
				{
					using (var fs = new FileStream(objPath, FileMode.Open))
					{
						res = LsnResourceThing.Read(fs,new string(objPath.Skip(4).Reverse().Skip(4).Reverse().ToArray()),ResourceLoader);
					}
				}
			}
			else if (Program.MakeResource(path, File.ReadAllText(srcPath), objPath, out res) != 0)
				throw new ApplicationException();
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
							res = LsnResourceThing.Read(fs, new string(objPath.Skip(4).Reverse().Skip(4).Reverse().ToArray()), ResourceLoader);
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
					Use(path);

					source = source.Replace(match.Value, ""); // TODO: Test.
					usePaths.Add(path);
				}
			}
			//_Environment = new LsnEnvironment(usePaths);

			if (source.Contains("#mut"))
			{
				Mutable = true;
				source = source.Replace("#mut", "");
			}
			return source;
		}

		protected void Use(string path)
		{
			LsnResourceThing res;
			if (path.StartsWith(@"Lsn Core\", StringComparison.Ordinal))
				res = ResourceManager.GetStandardLibraryResource(new string(path.Skip(9).ToArray()));
			else if (path.StartsWith(@"std\", StringComparison.Ordinal))
				res = ResourceManager.GetStandardLibraryResource(new string(path.Skip(4).ToArray()));
			else
				res = Load(path);

			Use(res, path);
		}

		protected void Use(LsnScriptBase resource, string path)
		{
			foreach (var u in resource.Usings)
			{
				Use(u);
			}

			foreach (var recType in resource.StructTypes.Values)
			{
				LoadedTypes.Add(recType);
				recType.Id.Load(recType);
			}
			foreach (var stType in resource.RecordTypes.Values)
			{
				LoadedTypes.Add(stType);
				stType.Id.Load(stType);
			}
			foreach (var hostInterface in resource.HostInterfaces.Values)
			{
				LoadedTypes.Add(hostInterface);
				LoadType(hostInterface);
				if(!HostInterfaces.ContainsKey(hostInterface.Name))
					HostInterfaces.Add(hostInterface.Name, hostInterface);
			}
			foreach (var scObj in resource.ScriptClassTypes.Values)
			{
				LoadedTypes.Add(scObj);
				scObj.Id.Load(scObj);
				LoadType(scObj);
				if(!ScriptClasses.ContainsKey(scObj.Name))
					ScriptClasses.Add(scObj.Name, scObj);
			}
			foreach (var pair in resource.Functions)
			{
				if (!_LoadedExternallyDefinedFunctions.ContainsKey(pair.Key))
				{
					_LoadedExternallyDefinedFunctions.Add(pair.Key, pair.Value);
					LoadFunctionParamAndReturnTypes(pair.Value.Signature);
				}
			}
			if(resource.GameValues != null)
			{
				foreach (var gameValue in resource.GameValues)
				{
					LoadedGameValues.Add(gameValue.Key, gameValue.Value);
				}
			}
			//ToDo: Generic types and functions...
			Usings.Add(path);
		}

		#region Functions
		private readonly Dictionary<string, Function> _LoadedExternallyDefinedFunctions = new Dictionary<string, Function>();
		protected IReadOnlyDictionary<string, Function> LoadedExternallyDefinedFunctions => _LoadedExternallyDefinedFunctions;

		public Function GetFunction(string name)
		{
			if (_LoadedExternallyDefinedFunctions.ContainsKey(name)) return _LoadedExternallyDefinedFunctions[name];

			throw new LsnrFunctionNotFoundException(Path, name);
		}
		#endregion

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
			if (type != null && type.Id.Type == null)
			{
				type.Id.Load(type);
				// Methods...
				foreach (var func in type.Methods.Values)
					LoadFunctionParamAndReturnTypes(func.Signature);

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
		/// The types that will not be included by the output.
		/// </summary>
		protected readonly IList<LsnType> LoadedTypes = LsnType.GetBaseTypes();
		protected readonly Dictionary<string, HostInterfaceType> HostInterfaces = new Dictionary<string, HostInterfaceType>();
		protected readonly Dictionary<string, HostInterfaceType> MyHostInterfaces = new Dictionary<string, HostInterfaceType>();
		protected readonly Dictionary<string, ScriptClass> ScriptClasses = new Dictionary<string, ScriptClass>();

		protected readonly IList<GenericType> LoadedGenerics = LsnType.GetBaseGenerics();

		public virtual bool TypeExists(string name)
			=> LoadedTypes.Any(t => t.Name == name);

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

				throw new LsnrTypeNotFoundException(Path, name);
			}
			var type = LoadedTypes.FirstOrDefault(t => t.Name == name);
			if (type != null) return type;

			if (type == null)
				throw new LsnrTypeNotFoundException(Path, name);
			return type;
		}

		public virtual bool UniqueScriptObjectTypeExists(string name)
			=> LoadedTypes.Any((t) => t.Name == name && ((t as ScriptClass)?.Unique ?? false));

		public virtual bool GenericTypeExists(string name)
			=> LoadedGenerics.Any(t => t.Name == name);

		public virtual GenericType GetGenericType(string name)
		{
			var type = LoadedGenerics.FirstOrDefault(t => t.Name == name);
			return type;
		}

		public virtual TypeId GetTypeId(string name) => GetType(name).Id;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "index")]
		public TypeId GetTypeId(ushort index)
		{
			throw new NotImplementedException();
		}

		public abstract void GenericTypeUsed(TypeId typeId);

		/*public virtual void AddType(LsnType type)
		{
			IncludedTypes.Add(type);
		}*/
		#endregion
	}
}
