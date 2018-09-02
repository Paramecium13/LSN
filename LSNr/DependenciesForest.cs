using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	class DependenciesNode
	{
		public readonly string Path;
		private List<DependenciesNode> _Dependencies = new List<DependenciesNode>();
		private List<DependenciesNode> _Dependents = new List<DependenciesNode>();

		/// <summary>
		/// No other file depends on this file if true.
		/// </summary>
		public bool IsLeaf => _Dependents.Count == 0;

		/// <summary>
		/// This file depends on nothing if true.
		/// </summary>
		public bool IsRoot => _Dependencies.Count == 0;

		public int Level { get; private set; } = -1;

		private DependenciesNode(string path) { Path = path; }

		private int CalculateLevel()
		{
			if (Level >= 0)
				return Level;
			if(IsRoot)
			{
				Level = 0;
				return 0;
			}
			int max = 0;
			foreach (var dep in _Dependencies)
			{
				var x = dep.CalculateLevel();
				if (x > max)
					max = x;
			}
			Level = max + 1;
			return Level;
		}

		public IEnumerable<DependenciesNode> Dependencies
		{
			get
			{
				foreach (var dep in _Dependencies)
					yield return dep;
			}
		}

		public IEnumerable<string> DependencyPaths
		{
			get
			{
				foreach (var dep in _Dependencies)
					yield return dep.Path;
			}
		}

		internal static ConcurrentDictionary<string, DependenciesNode> CreateForest(DependenciesFile dependencies, string[] changedFiles)
		{
			var deps = new ConcurrentDictionary<string, DependenciesNode>();
			Parallel.ForEach(changedFiles, path =>
			{
				deps.TryAdd(path, new DependenciesNode(path));
				if (!dependencies.Dependencies.ContainsKey(path))
					dependencies.Dependencies.TryAdd(path, DependenciesFile.ReadDependencies(path));
			});

			/*Parallel.ForEach(changedFiles, path =>
			{*/
			foreach (var path in changedFiles)
			{
				var node = deps[path];
				foreach (var dependency in dependencies.Dependencies[path])
					node._Dependencies.Add(deps[dependency]);
				foreach (var dependency in node._Dependencies)
				{
					lock (dependency._Dependents)
					{
						dependency._Dependents.Add(node);
					}
				}
			}//);
			/*foreach (var node in deps.Values)
				node.CalculateLevel();*/
			return deps;
		}
	}
}
