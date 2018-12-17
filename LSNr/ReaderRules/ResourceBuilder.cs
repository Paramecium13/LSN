using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;

namespace LSNr.ReaderRules
{
	class ResourceBuilder : IPreResource
	{
		readonly List<string> Usings = new List<string>();

		readonly Dictionary<string, LsnType>			LoadedTypes				= LsnType.GetBaseTypes().ToDictionary(t => t.Name);
		readonly Dictionary<string, Function>			LoadedFunctions			= new Dictionary<string, Function>();
		readonly Dictionary<string, GameValue>			LoadedGameValues		= new Dictionary<string, GameValue>();
		readonly Dictionary<string, HostInterfaceType>	LoadedHostInterfaces	= new Dictionary<string, HostInterfaceType>();
		readonly Dictionary<string, ScriptClass>		LoadedScriptClasses		= new Dictionary<string, ScriptClass>();

		readonly ScriptPartMap<LsnFunction, ISlice<Token>>			MyFunctions			= new ScriptPartMap<LsnFunction, ISlice<Token>>();
		readonly ScriptPartMap<ScriptClass, ISlice<Token>>			MyScriptClasses		= new ScriptPartMap<ScriptClass, ISlice<Token>>();
		readonly ScriptPartMap<HostInterfaceType, PreHostInterface>	MyHostInterfaces	= new ScriptPartMap<HostInterfaceType, PreHostInterface>();
		readonly ScriptPartMap<TypeId, ISlice<Token>>				MyStructs			= new ScriptPartMap<TypeId, ISlice<Token>>();
		readonly ScriptPartMap<TypeId, ISlice<Token>>				MyRecords			= new ScriptPartMap<TypeId, ISlice<Token>>();

		public string Path { get; private set; }

		public void RegisterUsing(string file)
		{
			Usings.Add(file);
		}
		#region Load
		readonly HashSet<string> LoadedResources = new HashSet<string>();
		protected void Use(string path)
		{
			Use(ResourceLoader(path), path);
			LoadedResources.Add(path);
		}

		protected void Use(LsnScriptBase resource, string path)
		{
			foreach (var u in resource.Usings)
			{
				if(!LoadedResources.Contains(u)) Use(u);
			}

			foreach (var recType in resource.StructTypes.Values)
			{
				LoadedTypes.Add(recType.Name,recType);
				recType.Id.Load(recType);
			}
			foreach (var stType in resource.RecordTypes.Values)
			{
				LoadedTypes.Add(stType.Name, stType);
				stType.Id.Load(stType);
			}
			foreach (var hostInterface in resource.HostInterfaces.Values)
			{
				LoadedTypes.Add(hostInterface.Name, hostInterface);
				LoadType(hostInterface);
				if (!LoadedHostInterfaces.ContainsKey(hostInterface.Name))
					LoadedHostInterfaces.Add(hostInterface.Name, hostInterface);
			}
			foreach (var scObj in resource.ScriptClassTypes.Values)
			{
				LoadedTypes.Add(scObj.Name, scObj);
				scObj.Id.Load(scObj);
				LoadType(scObj);
				if (!LoadedScriptClasses.ContainsKey(scObj.Name))
					LoadedScriptClasses.Add(scObj.Name, scObj);
			}
			foreach (var pair in resource.Functions)
			{
				if (!LoadedFunctions.ContainsKey(pair.Key))
				{
					LoadedFunctions.Add(pair.Key, pair.Value);
					LoadFunctionParamAndReturnTypes(pair.Value.Signature);
				}
			}
			if (resource.GameValues != null)
			{
				foreach (var gameValue in resource.GameValues)
				{
					LoadedGameValues.Add(gameValue.Key, gameValue.Value);
				}
			}
			//ToDo: Generic types and functions...
			Usings.Add(path);
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
					foreach (var method in hType.MethodDefinitions.Values)
						LoadFunctionParamAndReturnTypes(method);
					foreach (var ev in hType.EventDefinitions.Values)
						foreach (var param in ev.Parameters)
							LoadType(GetType(param.Type.Name));
				}
			}
		}
		#endregion

		public List<Parameter> ParseParameters(IReadOnlyList<Token> tokens)
		{
			throw new NotImplementedException();
		}

		public void RegisterFunction(LsnFunction function, ISlice<Token> body)
		{
			MyFunctions.AddPart(function.Name, function, body);
		}

		public Function GetFunction(string name) => MyFunctions.HasPart(name) ? MyFunctions.GetPart(name) : LoadedFunctions[name];

		public void RegisterHostInterface(string name, ISlice<Token> tokens)
		{
			throw new NotImplementedException();
		}

		public void RegisterRecordType(string name, ISlice<Token> tokens)
		{
			throw new NotImplementedException();
		}

		public void RegisterScriptClass(string name, string hostname, bool unique, string metadata, ISlice<Token> tokens)
		{
			throw new NotImplementedException();
		}

		public void RegisterStructType(string name, ISlice<Token> tokens)
		{
			throw new NotImplementedException();
		}


		public bool TypeExists(string name)
		{
			throw new NotImplementedException();
		}

		public bool GenericTypeExists(string name)
		{
			throw new NotImplementedException();
		}

		public void GenericTypeUsed(TypeId typeId)
		{
			throw new NotImplementedException();
		}

		public GenericType GetGenericType(string name)
		{
			throw new NotImplementedException();
		}

		public LsnType GetType(string name)
		{
			throw new NotImplementedException();
		}

		public TypeId GetTypeId(string name)
		{
			throw new NotImplementedException();
		}

		private static readonly Func<string, LsnResourceThing> ResourceLoader =
			(path) =>
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

		protected static LsnResourceThing Load(string path)
		{
			LsnResourceThing res = null;
			var objPath = Program.GetObjectPath(path);
			var srcPath = Program.GetSourcePath(path);
			using (var fs = File.OpenRead(objPath))
			{
				res = LsnResourceThing.Read(fs, new string(objPath.Skip(4).Reverse().Skip(4).Reverse().ToArray()), ResourceLoader);
			}
			return res;
		}
	}
}
