﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using Tokens;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

		protected IReadOnlyList<IToken> Tokens;


		public abstract Scope CurrentScope { get; set; }

		/// <summary>
		/// Are variables defined in this script mutable?
		/// </summary>
		public bool Mutable { get { return _Mutable; } protected set { _Mutable = value; } }

		/// <summary>
		/// Is this script valid (free of errors)?
		/// </summary>
		public bool Valid { get; set; } = true;


		public BasePreScript(string src)
		{
			Source = src;
		}

		/// <summary>
		/// 
		/// </summary>
		protected void Tokenize() { Tokens = Tokenizer.Tokenize(Text); }

		protected void Include(LsnResourceThing resource)
		{
			foreach(var pair in resource.Functions)
			{
				if (Functions.ContainsKey(pair.Key)) throw new ApplicationException();
				Functions.Add(pair.Key, pair.Value);
			}
			// ToDo: Add types.

		}

		protected LsnResourceThing Load(string path)
		{
			LsnResourceThing res;
			using (var fs = new FileStream(path, FileMode.Open))
			{
				res = (LsnResourceThing)(new BinaryFormatter().Deserialize(fs));
			}
			return res;
		}

		protected void Use(LsnResourceThing resource, string path)
		{
			foreach (var pair in resource.Functions)
			{
				if (LoadedExternalFunctions.ContainsKey(pair.Key)) throw new ApplicationException();
				LoadedExternalFunctions.Add(pair.Key, pair.Value);
			}
			// ToDo: Add types.
			Usings.Add(path);
		}		

		#region Functions

		/// <summary>
		/// The functions included in this script.
		/// </summary>
		protected readonly Dictionary<string, Function> Functions = new Dictionary<string, Function>();

		/// <summary>
		/// The external functions that to be linked at runtime.
		/// </summary>
		protected readonly Dictionary<string, Function> ExternalFunctions = new Dictionary<string, Function>();

		/// <summary>
		/// 
		/// </summary>
		private Dictionary<string, Function> LoadedExternalFunctions = new Dictionary<string, Function>();

		protected List<string> Usings = new List<string>();

		public bool FunctionExists(string name)
			=> Functions.ContainsKey(name) || LoadedExternalFunctions.ContainsKey(name);

		public bool FunctionIsIncluded(string name)
			=> Functions.ContainsKey(name);

		public Function GetFunction(string name)
		{
			if (Functions.ContainsKey(name)) return Functions[name];
			if (LoadedExternalFunctions.ContainsKey(name)) return AddExternalFunction(LoadedExternalFunctions[name]);
			else throw new ApplicationException($"Function \"{name}\" not found. Are you missing an include or using?");
		}

		private ExternalFunction AddExternalFunction(Function fn)
		{
			var exFn = new ExternalFunction(fn.Name, fn.Parameters.Select(p => new Parameter(p.Name, p.Type, p.DefaultValue, p.Index)).ToList(),
				fn.ReturnType);
			ExternalFunctions.Add(fn.Name, exFn);
			Functions.Add(fn.Name, exFn);
			return exFn;
		}


		
		protected void AddFunction(Function fn)
		{
			Functions.Add(fn.Name, fn);
		}


		#endregion

		#region Types

		public abstract LsnType GetType(string name);
		public abstract bool TypeExists(string name);
		public abstract bool GenericTypeExists(string name);
		public abstract GenericType GetGenericType(string name);
		#endregion
	}
}
