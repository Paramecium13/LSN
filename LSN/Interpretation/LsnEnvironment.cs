﻿using LsnCore.Types;
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
	[Serializable]
	public sealed class LsnEnvironment //TODO: Improve this!
	{
		// When a function from a used resource is loaded, the interpreter should enter the environment for that resource file.

		public static readonly LsnEnvironment Default = new LsnEnvironment();
		
		[NonSerialized]
		private Dictionary<string, Function> _Functions = new Dictionary<string, Function>();
		/// <summary>
		/// 
		/// </summary>
		public IReadOnlyDictionary<string, Function> Functions { get { return _Functions; }}

		[NonSerialized]
		private Dictionary<string, LsnStructType> _StructTypes = new Dictionary<string, LsnStructType>();
		public IReadOnlyDictionary<string, LsnStructType> StructTypes { get { return _StructTypes; } }

		[NonSerialized]
		private Dictionary<string, RecordType> _RecordTypes = new Dictionary<string, RecordType>();
		public IReadOnlyDictionary<string, RecordType> RecordTypes { get { return _RecordTypes; }}


		//Serialized
		private readonly IReadOnlyList<string> Resources;

		[NonSerialized]
		private bool Loaded = false;


		[NonSerialized]
		private readonly HashSet<string> LoadedResources = new HashSet<string>();

		/// <summary>
		/// Sets up the default environment.
		/// </summary>
		private LsnEnvironment()
		{
			/*_Functions.Add("Sqrt", LsnMath.Sqrt);
			_Functions.Add("Sin", LsnMath.Sin);
			_Functions.Add("Cos", LsnMath.Cos);
			_Functions.Add("Tan", LsnMath.Tan);*/
		}

		/// <summary>
		/// Sets up the environment for the provided script.
		/// </summary>
		/// <param name="script"></param>
		public LsnEnvironment(LsnScriptBase script)
			:this()
		{
			foreach (var pair in script.Functions) _Functions.Add(pair.Key, pair.Value);
			foreach (var pair in script.StructTypes) _StructTypes.Add(pair.Key, pair.Value);
			foreach (var pair in script.RecordTypes) _RecordTypes.Add(pair.Key, pair.Value);
			//foreach (var rs in script.Usings) LoadResource(rs + ".dat"); 
		}


		public LsnEnvironment(IEnumerable<string> resources)
		{
			Resources = resources.ToList();
		}


		public void Load(ILsnFileManager fileManager)
		{
			if (!Loaded)
			{
				foreach (var res in Resources)
				Load(fileManager.LoadResourse(res),fileManager);
			}
			else throw new InvalidOperationException("Already loaded");
		}


		private void Load(LsnScriptBase script, ILsnFileManager fileManager)
		{
			foreach (var pair in script.Functions) _Functions.Add(pair.Key, pair.Value);
			foreach (var pair in script.StructTypes) _StructTypes.Add(pair.Key, pair.Value);
			foreach (var pair in script.RecordTypes) _RecordTypes.Add(pair.Key, pair.Value);

			foreach (var res in script.Usings)
				if(!(Resources.Contains(res) || LoadedResources.Contains(res)))
					Load(fileManager.LoadResourse(res), fileManager);



		}
		

	}
}
