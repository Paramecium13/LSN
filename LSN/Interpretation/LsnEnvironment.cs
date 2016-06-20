using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// Contains the loaded functions and types.
	/// </summary>
	[Serializable]
	public class LsnEnvironment
	{
		// When a function from a used resource is loaded, the interpreter should enter the environment for that resource file.

		public static readonly LsnEnvironment Default = new LsnEnvironment();
		
		
		private Dictionary<string, Function> _Functions = new Dictionary<string, Function>();
		/// <summary>
		/// 
		/// </summary>
		public Dictionary<string, Function> Functions { get { return _Functions; } set { _Functions = value; } }

		private Dictionary<string, LsnStructType> _StructTypes = new Dictionary<string, LsnStructType>();
		public Dictionary<string, LsnStructType> StructTypes { get { return _StructTypes; } set { _StructTypes = value; } }

		/// <summary>
		/// Sets up the default environment.
		/// </summary>
		private LsnEnvironment()
		{
			Functions.Add("Sqrt", LsnMath.Sqrt);
			Functions.Add("Sin", LsnMath.Sin);
			Functions.Add("Cos", LsnMath.Cos);
			Functions.Add("Tan", LsnMath.Tan);
		}

		/// <summary>
		/// Sets up the environment for the provided script.
		/// </summary>
		/// <param name="script"></param>
		public LsnEnvironment(LsnScriptBase script)
			:this()
		{
			foreach (var pair in script.Functions) Functions.Add(pair.Key, pair.Value);
			foreach (var pair in script.StructTypes) StructTypes.Add(pair.Key, pair.Value);
			foreach (var rs in script.Usings) LoadResource(rs + ".dat"); 
		}

		/// <summary>
		/// Load the resource at the provided location.
		/// </summary>
		/// <param name="path"></param>
		private void LoadResource(string path)
		{
			var res = (LsnResourceThing)(new BinaryFormatter().Deserialize(new FileStream(path, FileMode.Open)));
			foreach(var pair in res.Functions) Functions.Add(pair.Key, pair.Value);
			foreach(var pair in res.StructTypes) StructTypes.Add(pair.Key, pair.Value);
		}

	}
}
