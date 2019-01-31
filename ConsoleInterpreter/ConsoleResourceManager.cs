using LsnCore;
using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleInterpreter
{
	class ConsoleResourceManager : ResourceManager
	{
		readonly string ObjPath;
		private const string ObjExtension = ".obj";

		readonly Dictionary<string, LsnResourceThing> Cache = new Dictionary<string, LsnResourceThing>();
		public ConsoleResourceManager(string objPath)
		{
			ObjPath = objPath;
		}

		public override IHostInterface GetHostInterface(uint id)
		{
			throw new NotImplementedException();
		}

		public override IHostInterface GetHostInterface(string id)
		{
			throw new NotImplementedException();
		}

		public override LsnType GetLsnType(string typeName)
		{
			throw new NotImplementedException();
		}

		public override ScriptObject GetScriptObject(uint scriptId)
		{
			throw new NotImplementedException();
		}

		public override ScriptObject GetScriptObject(string scriptId)
		{
			throw new NotImplementedException();
		}

		public override ScriptObject GetScriptObject(uint hostId, uint scriptId)
		{
			throw new NotImplementedException();
		}

		public override ScriptObject GetScriptObject(uint hostId, string scriptId)
		{
			throw new NotImplementedException();
		}

		public override ScriptObject GetScriptObject(string hostId, uint scriptId)
		{
			throw new NotImplementedException();
		}

		public override ScriptObject GetScriptObject(string hostId, string scriptId)
		{
			throw new NotImplementedException();
		}

		public override ScriptObject GetUniqueScriptObject(string name)
		{
			throw new NotImplementedException();
		}

		public override ScriptObject GetUniqueScriptObject(ScriptClass scriptClass)
		{
			throw new NotImplementedException();
		}

		public override LsnValue[] LoadValues(string id)
		{
			throw new NotImplementedException();
		}

		public override void SaveValues(LsnValue[] values, string id)
		{
			throw new NotImplementedException();
		}

		protected override LsnResourceThing GetResourceFromFile(string path)
		{
			if (Cache.ContainsKey(path))
				return Cache[path];
			LsnResourceThing resource;
			var p = Path.Combine(ObjPath, path) + ObjExtension;
			using (var fs = File.OpenRead(p))
			{
				resource = LsnResourceThing.Read(fs, p, GetResource);
			}
			Cache.Add(path, resource);
			return resource;
		}
	}
}
