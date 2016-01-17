using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// Contains the loaded functions and types.
	/// </summary>
	public class LSN_Environment
	{
		// When a function from a used resource is loaded, the interpreter should enter the environment for that resource file.

		private Dictionary<string, Function> _Functions = new Dictionary<string, Function>();
		/// <summary>
		/// 
		/// </summary>
		public Dictionary<string, Function> Functions { get { return _Functions; } set { _Functions = value; } }

		private Dictionary<string, LSN_StructType> _StructTypes = new Dictionary<string, LSN_StructType>();
		public Dictionary<string, LSN_StructType> StructTypes { get { return _StructTypes; } set { _StructTypes = value; } }

		/// <summary>
		/// Sets up the environment for the provided script.
		/// </summary>
		/// <param name="script"></param>
		public LSN_Environment(LSN_ScriptBase script)
		{
			foreach (var pair in script.Functions) Functions.Add(pair.Key, pair.Value);
			Functions.Add("Sqrt", LSN_Math.Sqrt);
			Functions.Add("Sin", LSN_Math.Sin);
			Functions.Add("Cos", LSN_Math.Cos);
			Functions.Add("Tan", LSN_Math.Tan);

			foreach (var pair in script.StructTypes) StructTypes.Add(pair.Key, pair.Value);
			foreach (var rs in script.Usings) LoadResource(rs + ".dat"); 
		}

		/// <summary>
		/// Load the resource at the provided location.
		/// </summary>
		/// <param name="path"></param>
		private void LoadResource(string path)
		{
			var res = (LSN_ResourceThing)(new BinaryFormatter().Deserialize(new FileStream(path, FileMode.Open)));
			foreach(var pair in res.Functions) Functions.Add(pair.Key, pair.Value);
			foreach(var pair in res.StructTypes) StructTypes.Add(pair.Key, pair.Value);
		}

	}
}
