using LsnCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LSNr
{
	class DependenciesFile
	{
		readonly ConcurrentDictionary<string, IList<string>> Dependencies;

		readonly ConcurrentDictionary<string, IList<string>> Dependents;

		private DependenciesFile(Dictionary<string, IList<string>> deps)
		{
			Dependencies = new ConcurrentDictionary<string, IList<string>>(deps);
			Dependents = new ConcurrentDictionary<string, IList<string>>();
			foreach (var pair in Dependencies)
				Dependents.TryAdd(pair.Key, new List<string>());
			foreach (var pair in Dependencies)
			{
				foreach (var dependency in pair.Value)
				{
					if (!Dependents[dependency].Contains(pair.Key))
						Dependents[dependency].Add(pair.Key);
				}
			}
		}

		internal void Write(string path)
		{
			using (var writer = new StreamWriter(File.Open(path, FileMode.Create)))
				writer.Write(JsonConvert.SerializeObject(Dependencies.OrderBy(p => p.Key), Formatting.Indented));
		}

		internal IEnumerable<string> GetUsed(string user) => Dependencies[user].ToArray();

		//  !!!!!!!!! Delete entries for changed files !!!!!!!
		internal IReadOnlyList<string> RegirsterChangedFiles(IEnumerable<string> paths)
		{
			var users = new List<string>();
			foreach (var path in paths)
			{
				var p = path.Substring(4, path.Length - 8);
				if (Dependencies.ContainsKey(path))
					Dependencies[p].Clear();
				else
				{
					Dependencies.TryAdd(p, new List<string>());
					Dependents.TryAdd(p, new List<string>());
				}
				users.AddRange(Dependents[p]);
			}
			return users.Distinct().ToList();
		}

		internal void RegisterDependency(string child, string parent, string first)
		{
			if(child == parent)
				throw new ApplicationException($"Circular dependency from '{child}' to '{first}'!!!");
			var parents = Dependencies[child];
			var updated = false;
			string[] grandparents = null;
			lock (parents)
			{
				updated = !parents.Contains(parent);
				if (updated)
				{
					parents.Add(parent);
					grandparents = Dependencies[parent].ToArray();
				}
			}
			if(updated)
			{
				var children = Dependents[parent];
				lock (children)
				{
					children.Add(child);
				}
				foreach (var grandchild in Dependents[child].ToArray())
				{
					RegisterDependency(grandchild, parent, parent);
				}
				foreach (var grandparent in grandparents)
				{
					RegisterDependency(child, grandparent, parent);
				}
			}

			// Dependencies[child] contains parent
			// Dependencies[child] contains every value in Dependencies[parent]
			//
			// Dependents[parent] contains child
			// for every value v in Dependents[child]: Dependencies[v] contains parent
		}

		internal static DependenciesFile Read(string path)
		{
			var json = (JObject)JToken.Parse(File.ReadAllText(path));
			var deps = new Dictionary<string, IList<string>>();
			List<string> ls;
			foreach (var item in json)
			{
				if (item.Value.Type == JTokenType.String)
					ls = new List<string> { item.Value.Value<string>() };
				else if (item.Value.Type == JTokenType.Array)
				{
					if(!item.Value.All(v => v.Type == JTokenType.String))
						throw new ApplicationException("Invalid Dependencies File");
					ls = item.Value.Values<string>().ToList();
				}
				else throw new ApplicationException("Invalid Dependencies File");

				deps.Add(item.Key, ls);
			}

			return new DependenciesFile(deps);
		}

		internal static DependenciesFile SetUp()
		{
			var deps = Directory.EnumerateFiles("src", "*.lsn", SearchOption.AllDirectories)
				.AsParallel()
				.Select(path => new KeyValuePair<string, IList<string>>(path.Substring(4,path.Length - 8), new List<string>()))
				.ToDictionary();

			return new DependenciesFile(deps);
		}
	}
}
