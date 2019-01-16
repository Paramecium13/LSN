using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	class DependencyWaiter
	{
		readonly IReadOnlyDictionary<string, Task> Tasks;

		readonly Dictionary<string, HashSet<string>> Usings = new Dictionary<string, HashSet<string>>();

		readonly DependenciesFile File;

		internal DependencyWaiter(DependenciesFile file, IReadOnlyDictionary<string, Task> tasks)
		{
			File = file;
			Tasks = tasks;
			foreach (var path in Tasks.Keys)
				Usings.Add(path, new HashSet<string>());
		}

		void AddDependency(string user, string used, string firstUsed)
		{
			if (user == used) throw new ApplicationException($"Circular dependency from '{user}' to '{firstUsed}'!!!");
			if (!Usings.ContainsKey(user))
				Usings.Add(user, new HashSet<string>());
			if (!Usings.ContainsKey(used))
				Usings.Add(used, new HashSet<string>());
			if (!Usings[user].Contains(used))
			{
				Usings[user].Add(used);
				foreach (var item in File.GetUsed(used))
				{
					AddDependency(user, item, firstUsed);
					if (Tasks.ContainsKey(item))
						Tasks[item].Wait();
				}
			}
		}

		internal void WaitOn(string waitingPath, string pathToWaitOn)
		{
			lock (this)
			{
				File.RegisterDependency(waitingPath, pathToWaitOn, pathToWaitOn);
				AddDependency(waitingPath, pathToWaitOn, pathToWaitOn);
			}
			if(Tasks.ContainsKey(pathToWaitOn))
				Tasks[pathToWaitOn].Wait();
		}
	}
}
