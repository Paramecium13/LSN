using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// Contains the loaded functions and types.
	/// </summary>
	public sealed class LsnEnvironment //TODO: Improve this!
	{
		// When a function from a used resource is loaded, the interpreter should enter the environment for that resource file.

		public static readonly LsnEnvironment Default = new LsnEnvironment();

		private Dictionary<string, Function> _Functions = new Dictionary<string, Function>();
		/// <summary>
		/// ...
		/// </summary>
		public IReadOnlyDictionary<string, Function> Functions { get { return _Functions; }}

		/*[NonSerialized]
		private Dictionary<string, RecordType> _StructTypes = new Dictionary<string, RecordType>();
		public IReadOnlyDictionary<string, RecordType> StructTypes { get { return _StructTypes; } }

		[NonSerialized]
		private Dictionary<string, StructType> _RecordTypes = new Dictionary<string, StructType>();
		public IReadOnlyDictionary<string, StructType> RecordTypes { get { return _RecordTypes; }}*/

		private Dictionary<string, ScriptClass> _ScriptObjectTypes = new Dictionary<string, ScriptClass>();
		public IReadOnlyDictionary<string, ScriptClass> ScriptObjectTypes => _ScriptObjectTypes;

		private readonly IReadOnlyList<string> Resources;

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

		/// <summary>
		/// Only used in LSNr!!!
		/// </summary>
		/// <param name="resources"></param>
		public LsnEnvironment(IEnumerable<string> resources)
		{
			Resources = resources.ToList();
		}

		private void Load(LsnScriptBase script, IResourceManager fileManager)
		{
			foreach (var pair in script.Functions)
				_Functions.Add(pair.Key, pair.Value);
			foreach (var pair in script.ScriptClassTypes)
				_ScriptObjectTypes.Add(pair.Key, pair.Value);
			foreach (var res in script.Usings)
			{
				if (!LoadedResources.Contains(res))
				{
					LoadedResources.Add(res);
					Load(fileManager.GetResource(res), fileManager);
				}
			}
		}
	}
}
