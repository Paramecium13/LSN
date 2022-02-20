using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using LsnCore.Runtime.Types;

namespace LsnCore
{
	/// <summary>
	/// Contains the loaded functions and types.
	/// </summary>
	public sealed class LsnEnvironment //TODO: Improve this!
	{
		// When a function from a used resource is loaded, the interpreter should enter the environment for that resource file.

		public static readonly LsnEnvironment Default = new LsnEnvironment();

		private readonly Dictionary<string, Function> _Functions = new Dictionary<string, Function>();
		/// <summary>
		/// ...
		/// </summary>
		public IReadOnlyDictionary<string, Function> Functions => _Functions;

		private readonly Dictionary<string, ScriptClass> _ScriptClasses = new Dictionary<string, ScriptClass>();
		public IReadOnlyDictionary<string, ScriptClass> ScriptClasses => _ScriptClasses;

		private readonly HashSet<string> LoadedResources = new HashSet<string>();

		/// <summary>
		/// Sets up the default environment.
		/// </summary>
		private LsnEnvironment(){}

		/// <summary>
		/// Sets up the environment for the provided script.
		/// </summary>
		/// <param name="script"></param>
		public LsnEnvironment(LsnScriptBase script,IResourceManager resourceManager)
			:this()
		{
			Load(script, resourceManager);
		}

		private void Load(LsnScriptBase script, IResourceManager fileManager)
		{
			foreach (var pair in script.Functions)
				_Functions.Add(pair.Key, pair.Value);
			foreach (var pair in script.ScriptClassTypes)
				_ScriptClasses.Add(pair.Key, pair.Value);
			foreach (var res in script.Usings)
			{
				if (LoadedResources.Contains(res)) continue;
				LoadedResources.Add(res);
				Load(fileManager.GetResource(res), fileManager);
			}
		}
	}
}
