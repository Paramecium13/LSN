﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ControlStructures;
using LSNr.Optimization;
using LSNr.Statements;

namespace LSNr.ReaderRules
{
	class ResourceBuilder : IPreResource, IPreScript
	{
		readonly List<string> Usings = new List<string>();

		readonly Dictionary<string, LsnType>			LoadedTypes				= LsnType.GetBaseTypes().ToDictionary(t => t.Name);
		readonly Dictionary<string, GenericType>		LoadedGenerics			= LsnType.GetBaseGenerics().ToDictionary(t => t.Name);
		readonly Dictionary<string, Function>			LoadedFunctions			= new Dictionary<string, Function>();
		readonly Dictionary<string, GameValue>			LoadedGameValues		= new Dictionary<string, GameValue>();
		readonly Dictionary<string, HostInterfaceType>	LoadedHostInterfaces	= new Dictionary<string, HostInterfaceType>();
		readonly Dictionary<string, ScriptClass>		LoadedScriptClasses		= new Dictionary<string, ScriptClass>();

		readonly List<TypeId> UsedGenerics = new List<TypeId>();

		readonly Dictionary<string, TypeId> MyTypes = new Dictionary<string, TypeId>();

		readonly Dictionary<string, Function>			MyFunctions				= new Dictionary<string, Function>();
		readonly List<StructType>						GeneratedStructTypes	= new List<StructType>();
		readonly List<RecordType>						GeneratedRecordTypes	= new List<RecordType>();
		readonly List<HandleType>						GeneratedHandleTypes	= new List<HandleType>();
		readonly Dictionary<string, ScriptClass>		GeneratedScriptClasses	= new Dictionary<string, ScriptClass>();
		readonly Dictionary<string, HostInterfaceType>	GeneratedHostInterfaces	= new Dictionary<string, HostInterfaceType>();

		readonly string PurePath;
		public string Path { get; private set; }
		public IPreScript Script => this;

		public IScope CurrentScope { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
		public bool Mutable => false;
		public bool Valid { get; set; } = true;

		public IReadOnlyList<IStatementRule> StatementRules
			=> throw new NotImplementedException();

		public IReadOnlyList<ControlStructureRule> ControlStructureRules => throw new NotImplementedException();

		internal ResourceBuilder(string path)
		{
			Path = path; PurePath = new string(path.Skip(4).Take(Path.Length - 8).ToArray());
		}

		public event Action<IPreResource> ParseSignaturesA;
		public event Action<IPreResource> ParseSignaturesB;

		public event Action<IPreResource> ParseProcBodies;
		/// <summary>
		/// Called after all type names have been registered.
		/// </summary>
		public LsnResourceThing Parse()
		{
			// All type names have been registered.

			// Parse Signatures:
			ParseSignaturesA?.Invoke(this);
			ParseSignaturesB?.Invoke(this);

			// End Parse Signatures

			// Parse Procedure Bodies:
			ParseProcBodies?.Invoke(this);
			// End Parse Procedure Bodies

			return GenerateResource();
		}

		public void RegisterUsing(string file)
		{
			//Usings.Add(file);
			Use(file);
		}
		#region Load
		readonly HashSet<string> LoadedResources = new HashSet<string>();
		protected void Use(string path)
		{
			if (!LoadedResources.Contains(path))
			{
				Use(Program.Load(PurePath, path), path);
				LoadedResources.Add(path);
			}
		}

		protected void Use(LsnScriptBase resource, string path)
		{
			foreach (var u in resource.Usings)
			{
				if (!LoadedResources.Contains(u)) Use(u);
			}

			foreach (var recType in resource.StructTypes.Values)
			{
				LoadedTypes.Add(recType.Name, recType);
				recType.Id.Load(recType);
			}
			foreach (var stType in resource.RecordTypes.Values)
			{
				LoadedTypes.Add(stType.Name, stType);
				stType.Id.Load(stType);
			}
			foreach (var handle in resource.HandleTypes)
				LoadedTypes.Add(handle.Name, handle);
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
				// Generics!!!
				if (!TypeExists(func.ReturnType.Name))
					throw new LsnrTypeNotFoundException(Path, func.ReturnType.Name);
				func.ReturnType.Load(GetType(func.ReturnType.Name));
				LoadType(func.ReturnType.Type);
			}
			foreach (var param in func.Parameters)
			{
				// Generics!!!
				if (!TypeExists(param.Type.Name))
					throw new LsnrTypeNotFoundException(Path, param.Type.Name);
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
		#region Register
		public void RegisterTypeId(TypeId id) { MyTypes.Add(id.Name, id); }

		public void RegisterFunction(Function fn)
		{
			MyFunctions.Add(fn.Name, fn);
		}

		public IProcedure CreateFunction(IReadOnlyList<Parameter> args, TypeId retType, string name, bool isVirtual = false)
		{
			if (isVirtual)
				throw new InvalidOperationException();
			var fn = new LsnFunction(args, retType, name, Path);
			RegisterFunction(fn);
			return fn;
		}

		public void RegisterStructType(StructType structType)		=> GeneratedStructTypes.Add(structType);
		public void RegisterRecordType(RecordType recordType)		=> GeneratedRecordTypes.Add(recordType);
		public void RegisterHandleType(HandleType handleType)		=> GeneratedHandleTypes.Add(handleType);
		public void RegisterHostInterface(HostInterfaceType host)	=> GeneratedHostInterfaces.Add(host.Name, host);
		public void RegisterScriptClass(ScriptClass scriptClass)	=> GeneratedScriptClasses.Add(scriptClass.Name, scriptClass);
		#endregion

		public Function GetFunction(string name) => MyFunctions.ContainsKey(name) ? MyFunctions[name] : LoadedFunctions[name];

		public bool TypeExists(string name)
		{
			if (name.Contains('`'))
			{
				var names = name.Split('`');
				if (GenericTypeExists(names[0])) return true;
				return false;
			}
			return LoadedTypes.ContainsKey(name) || MyTypes.ContainsKey(name);
		}

		public bool GenericTypeExists(string name) => LoadedGenerics.ContainsKey(name);

		public void GenericTypeUsed(TypeId typeId)
		{
			if (!UsedGenerics.Contains(typeId))
				UsedGenerics.Add(typeId);
		}

		public GenericType GetGenericType(string name) => LoadedGenerics[name];

		public LsnType GetType(string name) {
			if (name == null)
				throw new ApplicationException();
			if (name.Contains('`'))
			{
				var names = name.Split('`');
				if (GenericTypeExists(names[0]))
				{
					var generic = GetGenericType(names[0]);
					return generic.GetType(names.Skip(1).Select(GetType).Select(t => t.Id).ToArray());
				}

				throw new LsnrTypeNotFoundException(Path, name);
			}
			return LoadedTypes.ContainsKey(name) ? LoadedTypes[name] : MyTypes[name].Type;
		}

		public TypeId GetTypeId(string name)
		{
			try
			{
				return LoadedTypes.ContainsKey(name) ? LoadedTypes[name].Id : MyTypes[name];
			}
			catch (Exception)
			{
				throw;
			}
		}

		public SymbolType CheckSymbol(string symbol)
		{
			if (MyFunctions.ContainsKey(symbol) || LoadedFunctions.ContainsKey(symbol))
				return SymbolType.Function;
			if ((LoadedTypes.ContainsKey(symbol) && ((LoadedTypes[symbol] as ScriptClass)?.Unique ?? false))
				|| (GeneratedScriptClasses.ContainsKey(symbol) && GeneratedScriptClasses[symbol].Unique))// check MyScriptClasses
				return SymbolType.UniqueScriptObject;
			if (TypeExists(symbol))
				return SymbolType.Type;

			return SymbolType.Undefined;
		}

		TypeId[] GetTypeIds()
		{
			return new List<TypeId> { new TypeId("void") }
				.Union(LoadedTypes.Values.Select(t => t.Id))
				.Union(MyTypes.Values)
				// I don't think I need these; they should already be in MyTypes.
				/*.Union(MyRecords.SelectFromParts(p=>p))
				.Union(MyStructs.SelectFromParts(p =>p))
				//.Union(MyHostInterfaces.SelectFromParts(p => p))
				//.Union(MyScriptClasses.SelectFromParts(p => p))*/
				.Union(UsedGenerics)
				.Distinct()
				.ToArray();
		}

		LsnResourceThing GenerateResource() => new LsnResourceThing(GetTypeIds())
		{
			Functions = MyFunctions,
			GameValues = new Dictionary<string, GameValue>(),
			HandleTypes = GeneratedHandleTypes,
			HostInterfaces = GeneratedHostInterfaces,
			RecordTypes = GeneratedRecordTypes.ToDictionary(r => r.Name),
			ScriptClassTypes = GeneratedScriptClasses,
			StructTypes = GeneratedStructTypes.ToDictionary(s => s.Name),
			Usings = Usings
		};

		public IReadOnlyList<Parameter> ParseParameters(IReadOnlyList<Token> tokens) => ((IFunctionContainer)this).BaseParseParameters(tokens);
	}
}
