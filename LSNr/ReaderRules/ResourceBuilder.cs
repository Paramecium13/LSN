﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.Optimization;

namespace LSNr.ReaderRules
{
	class FunctionSource
	{
		internal readonly ISlice<Token> Args;
		internal readonly ISlice<Token> ReturnType;
		internal readonly ISlice<Token> Body;
		internal FunctionSource(ISlice<Token> args, ISlice<Token> ret, ISlice<Token> body)
			{ Args = args; ReturnType = ret; Body = body;}
	}

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

		readonly Dictionary<string, FunctionSource> FunctionSources = new Dictionary<string, FunctionSource>();

		readonly Dictionary<string, TypeId> MyTypes = new Dictionary<string, TypeId>();

		readonly ScriptPartMap<LsnFunction, ISlice<Token>>			MyFunctions			= new ScriptPartMap<LsnFunction, ISlice<Token>>();
		readonly ScriptPartMap<TypeId, ISlice<Token>>				MyStructs			= new ScriptPartMap<TypeId, ISlice<Token>>();
		readonly ScriptPartMap<TypeId, ISlice<Token>>				MyRecords			= new ScriptPartMap<TypeId, ISlice<Token>>();
		//readonly ScriptPartMap<ScriptClass, ISlice<Token>>			MyScriptClasses		= new ScriptPartMap<ScriptClass, ISlice<Token>>();
		//readonly ScriptPartMap<HostInterfaceType, PreHostInterface>	MyHostInterfaces	= new ScriptPartMap<HostInterfaceType, PreHostInterface>();

		readonly List<StructType>			GeneratedStructTypes	= new List<StructType>();
		readonly List<RecordType>			GeneratedRecordTypes	= new List<RecordType>();
		//readonly List<ScriptClass>			GeneratedScriptClasses	= new List<ScriptClass>();
		//readonly List<HostInterfaceType>	GeneratedHostInterfaces	= new List<HostInterfaceType>();

		public string Path { get; private set; }

		public IScope CurrentScope { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool Mutable => false;
		public bool Valid { get; set; }

		internal ResourceBuilder(string path)
		{
			Path = path;
		}

		/// <summary>
		/// Called after all stuff has been registered.
		/// </summary>
		public LsnResourceThing Parse()
		{
			ParseFunctionSignatures();

			ParseRecords();
			ParseStructs();

			// ParseHostInterfaces();
			// PreParseScriptClasses();

			ParseFunctions();

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
		#region Register
		public void RegisterFunction(string name, ISlice<Token> args, ISlice<Token> returnType, ISlice<Token> body)
		{
			FunctionSources.Add(name, new FunctionSource(args, returnType, body));
		}

		public void RegisterRecordType(string name, ISlice<Token> tokens)
		{
			var id = new TypeId(name);
			MyRecords.AddPart(name, id, tokens);
			MyTypes.Add(name, id);
		}

		public void RegisterStructType(string name, ISlice<Token> tokens)
		{
			var id = new TypeId(name);
			MyStructs.AddPart(name, id, tokens);
			MyTypes.Add(name, id);
		}

		public void RegisterScriptClass(string name, string hostname, bool unique, string metadata, ISlice<Token> tokens)
		{
			throw new NotImplementedException();
		}

		public void RegisterHostInterface(string name, ISlice<Token> tokens)
		{
			throw new NotImplementedException();
		}
		#endregion
		#region PreParse
		void ParseFunctionSignatures()
		{
			foreach (var fnSrc in FunctionSources)
			{
				var parameters = ParseParameters(fnSrc.Value.Args);
				TypeId returnType = null;
				if(fnSrc.Value.ReturnType != null)
				{
					try { returnType = this.ParseTypeId(fnSrc.Value.ReturnType, 0, out int i); if (i < 0) throw new ApplicationException(); }
					catch (Exception e)
					{
						throw new LsnrParsingException(fnSrc.Value.ReturnType[0],$"Error parsing return type for function '{fnSrc.Key}'.",e,Path);
					}
				}
				var fn = new LsnFunction(parameters, returnType, fnSrc.Key, Path);
				MyFunctions.AddPart(fnSrc.Key, fn, fnSrc.Value.Body);
			}
		}

		Tuple<string, TypeId>[] ParseFields(ISlice<Token> tokens)
		{
			if (tokens.Length < 3) // struct Circle { Radius : double}
			{
				throw new LsnrParsingException(tokens[0], "too few tokens.", Path);
			}
			var fields = new List<Tuple<string, TypeId>>();
			for (int i = 0; i < tokens.Length; i++)
			{
				var fName = tokens[i++].Value; // Get the name of the field, move on to the next token.
				if (i >= tokens.Length) // Make sure the definition does not end..
					throw new LsnrParsingException(tokens[i - 1], "unexpected end of declaration, expected ':'.", Path);
				if (tokens[i++].Value != ":") // Make sure the next token is ':', move on to the next token.
					throw LsnrParsingException.UnexpectedToken(tokens[i - 1], ":", Path);
				if (i >= tokens.Length) // Make sure the definition does not end.
					throw new LsnrParsingException(tokens[i - 1], "unexpected end of declaration, expected type.", Path);
				var tId = this.ParseTypeId(tokens, i, out i);
				fields.Add(new Tuple<string, TypeId>(fName, tId));
				if (i + 1 < tokens.Length && tokens[i].Value == ",") // Check if the definition ends, move on to the next token
																	 // and check that it is ','.
				{
					// Move on to the next token, which should be the name of the next field.
					continue; // Move on to the next field.
				}
				break;
			}
			return fields.ToArray();
		}

		void ParseStructs()
		{
			foreach (var (name, id, src) in MyStructs)
			{
				var fields = ParseFields(src);
				var str = new StructType(id, fields); // also loads the type into the id
				GeneratedStructTypes.Add(str);
			}
		}

		void ParseRecords()
		{
			foreach (var (name, id, src) in MyRecords)
			{
				var fields = ParseFields(src);
				var rec = new RecordType(id, fields); // also loads the type into the id
				GeneratedRecordTypes.Add(rec);
			}
		}
		#endregion

		void ParseFunctions()
		{
			foreach (var (name, fn, src) in MyFunctions)
			{
				try
				{
					var preFn = new PreFunction(this);
					foreach (var param in fn.Parameters)
						preFn.CurrentScope.CreateVariable(param);
					var parser = new Parser(src, preFn);
					parser.Parse();
					preFn.CurrentScope.Pop(parser.Components);
					if (preFn.Valid)
					{
						var cmps = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
						fn.Code = new ComponentFlattener().Flatten(cmps);
						fn.StackSize = (preFn.CurrentScope as VariableTable)?.MaxSize ?? -1;
					}
					else
						Valid = false;
				}
				catch (LsnrException e)
				{
					Logging.Log("function", name, e);
					Valid = false;
				}
				catch (Exception e)
				{
					Logging.Log("function", name, e, Path);
					Valid = false;
				}
			}
		}

		public Function GetFunction(string name) => MyFunctions.HasPart(name) ? MyFunctions.GetPart(name) : LoadedFunctions[name];

		public bool TypeExists(string name) => LoadedTypes.ContainsKey(name) || MyTypes.ContainsKey(name);

		public bool GenericTypeExists(string name) => LoadedGenerics.ContainsKey(name);

		public void GenericTypeUsed(TypeId typeId)
		{
			if (!UsedGenerics.Contains(typeId))
				UsedGenerics.Add(typeId);
		}

		public GenericType GetGenericType(string name) => LoadedGenerics[name];

		public LsnType GetType(string name) => LoadedTypes.ContainsKey(name) ? LoadedTypes[name] : MyTypes[name].Type;

		public TypeId GetTypeId(string name) => LoadedTypes.ContainsKey(name) ? LoadedTypes[name].Id : MyTypes[name];

		public SymbolType CheckSymbol(string name)
		{
			if (MyFunctions.HasPart(name) || LoadedFunctions.ContainsKey(name))
				return SymbolType.Function;
			if (LoadedTypes.ContainsKey(name) && ((LoadedTypes[name] as ScriptClass)?.Unique ?? false))// check MyScriptClasses
				return SymbolType.UniqueScriptObject;
			if (TypeExists(name))
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

		LsnResourceThing GenerateResource()
		{
			return new LsnResourceThing(GetTypeIds()) {
				Functions = MyFunctions.ToDictionary(e => e.name, e => e.Part as Function),
				GameValues = new Dictionary<string, GameValue>(),
				HostInterfaces = new Dictionary<string, HostInterfaceType>(),
				Includes = new List<string>(),
				RecordTypes = GeneratedRecordTypes.ToDictionary(r => r.Name),
				ScriptClassTypes = new Dictionary<string,ScriptClass>(),
				StructTypes = GeneratedStructTypes.ToDictionary(s => s.Name),
				Usings = Usings
			};
			throw new NotImplementedException();
		}

		static readonly Func<string, LsnResourceThing> ResourceLoader =
			(path) =>
			{
				if (path.StartsWith(@"Lsn Core\", StringComparison.Ordinal))
					return ResourceManager.GetStandardLibraryResource(new string(path.Skip(9).ToArray()));
				if (path.StartsWith(@"std\", StringComparison.Ordinal))
					return ResourceManager.GetStandardLibraryResource(new string(path.Skip(4).ToArray()));
				var objPath = Program.GetObjectPath(path);
				using (var stream = File.OpenRead(objPath)) //ToDo: Replace with a call to a static method...
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