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
		internal readonly ConcurrentDictionary<string, IReadOnlyList<string>> Dependencies;

		private DependenciesFile(Dictionary<string, IReadOnlyList<string>> deps)
		{
			Dependencies = new ConcurrentDictionary<string, IReadOnlyList<string>>(deps);
		}

		internal void Write(string path)
		{
			using (var writer = new StreamWriter(File.Open(path, FileMode.Create)))
				writer.Write(JsonConvert.SerializeObject(Dependencies, Formatting.Indented));
		}

		internal static DependenciesFile Read(string path)
		{
			var json = (JObject)JToken.Parse(File.ReadAllText(path));
			var deps = new Dictionary<string, IReadOnlyList<string>>();
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
				.Select(path => new KeyValuePair<string, IReadOnlyList<string>>(path, ReadDependencies(path)))
				.ToDictionary();

			return new DependenciesFile(deps);
		}

		internal static IReadOnlyList<string> ReadDependencies(string path)
		{
			var usings = new List<string>();
			using (var sr = new StreamReader(File.OpenRead(Program.GetSourcePath(path))))
			{
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (Regex.IsMatch(line, "#using", RegexOptions.IgnoreCase))
					{
						var u = Regex.Match(line, "#using\\s+\"(.+)\"").Groups
							.OfType<object>()
							.Select(o => o.ToString())
							.Skip(1)
							.First();
						if (!u.StartsWith(@"Lsn Core\", StringComparison.Ordinal) &&
								!u.StartsWith(@"std\", StringComparison.Ordinal))
							usings.Add(Program.GetSourcePath(u));
					}
				}
			}
			return usings;
		}
	}
}
