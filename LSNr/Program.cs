﻿using LsnCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LSNr
{
	static class Program
	{
		private const int NO_FILE = -1;
		private const int FILE_NOT_FOUND = -2;
		private const int ERROR_IN_SOURCE = -3;
		private const int CONFIG_EXISTS = -4;
		private const string DEP_FILE_PATH = "dependencies.json";

		private static MainFile _MainFile;

		internal static MainFile MainFile => _MainFile;
		private static EventWaitHandle MyWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
		private static DependenciesFile _DependenciesFile;

		static int Main(string[] args)
		{
			/*if (args.Length == 0 || string.Equals(args[0], "setup", StringComparison.OrdinalIgnoreCase)) // Set up the workspace.
			{
				return SetUp();
			}*/
			LssParser.ExpressionParser.DefaultSetUp();
			if (!File.Exists(args[0]))
			{
				return -1;
			}
			_MainFile = new MainFile(args[0]);
			Directory.SetCurrentDirectory(new FileInfo(args[0]).DirectoryName);

			if (File.Exists(DEP_FILE_PATH))
				_DependenciesFile = DependenciesFile.Read(DEP_FILE_PATH);
			else
				_DependenciesFile = DependenciesFile.SetUp();

			var changedFiles = GetChangedFiles();
			var dependenciesForest = DependenciesNode.CreateForest(_DependenciesFile, changedFiles);

			var tasks = new Dictionary<string, Task>();
			foreach (var path in changedFiles)
				tasks[path] = Task.Run(() => Reify(dependenciesForest[path], tasks));
			DependencyWaiter = new DependencyWaiter(tasks);
			MyWaitHandle.Set();

			var rTask = Task.WhenAll(tasks.Values);

			rTask.Wait();

			_DependenciesFile.Write(DEP_FILE_PATH);

			if (rTask.Status == TaskStatus.Faulted)
				return 8;

			return 0;
		}

		private static string[] GetChangedFiles()
		{
			var changed = new List<string>();
			foreach (var path in Directory.EnumerateFiles("src", "*.lsn", SearchOption.AllDirectories))
			{
				var objPath = GetObjectPath(path);
				if (!File.Exists(objPath))
					changed.Add(path);
				else
				{
					var srcUpdateTime = File.GetLastWriteTimeUtc(path);
					var objUpdateTime = File.GetLastWriteTimeUtc(objPath);
					if (srcUpdateTime < objUpdateTime)
						changed.Add(path);
				}
			}
			return changed.ToArray();
		}

		private static int SetUp()
		{
			// ...
			Directory.CreateDirectory("src");
			Directory.CreateDirectory("obj");

			return 0;
		}

		public static string GetObjectPath(string rawPath)
		{
			if(rawPath.StartsWith("obj"))
			{
				if (Path.HasExtension(rawPath))
					return rawPath;
				return rawPath + _MainFile.ObjectFileExtension;
			}
			if(rawPath.StartsWith("src"))
			{
				rawPath = new string(rawPath.Skip(4).ToArray());
			}
			if(Path.HasExtension(rawPath))
			{
				if(Path.GetExtension(rawPath) != _MainFile.ObjectFileExtension)
				{
					rawPath = new string(rawPath.Take(rawPath.Length - Path.GetExtension(rawPath).Length).Concat(_MainFile.ObjectFileExtension).ToArray());
				}
				return Path.Combine("obj",rawPath);
			}

			return Path.Combine("obj", rawPath + _MainFile.ObjectFileExtension);
		}

		public static string GetSourcePath(string rawPath)
		{
			if (!rawPath.EndsWith(".lsn"))
				rawPath += ".lsn";
			if (!rawPath.StartsWith("src\\"))
				rawPath = Path.Combine("src", rawPath);
			return rawPath;
		}

		public static void ReifyB(DependenciesNode myNode, IReadOnlyDictionary<string, Task> tasks)
		{
			var deps = new HashSet<string>(myNode.DependencyPaths);
			MyWaitHandle.WaitOne();
			foreach (var path in deps)
				tasks[path].Wait();

			// Parse the file
			var source = File.ReadAllText(GetSourcePath(myNode.Path));
			if (Regex.IsMatch(source, "#using", RegexOptions.IgnoreCase))
			{
				var updateDepsFile = false;
				var usings = new List<string>();
				foreach (var match in Regex.Matches(source, "#using\\s+\"(.+)\"").Cast<Match>())
				{
					var u = match.Groups.OfType<object>().Select(o => o.ToString()).Skip(1).First();
					if (!u.StartsWith(@"Lsn Core\", StringComparison.Ordinal) &&
							!u.StartsWith(@"std\", StringComparison.Ordinal))
						usings.Add(GetSourcePath(u));
				}

				foreach (var path in usings)
				{
					if (!deps.Contains(path))
					{
						tasks[path].Wait();
						updateDepsFile = true;
					}
				}
				if (updateDepsFile)
					_DependenciesFile.Dependencies[myNode.Path] = usings;
			}
			var rs = new PreResource(source, GetSourcePath(myNode.Path));
			rs.Reify();
			// Save the file.
			LsnResourceThing res;
			if (!rs.Valid)
			{
				res = null;
				Console.WriteLine("Invalid source.");
				Console.ReadLine();
				throw new ApplicationException();
			}
			res = rs.GetResource();
			using (var fs = File.Create(GetObjectPath(myNode.Path)))
			{
				res.Serialize(fs);
			}
		}

		private static DependencyWaiter DependencyWaiter;

		public static void Reify(DependenciesNode myNode, IReadOnlyDictionary<string, Task> tasks)
		{
			var deps = new HashSet<string>(myNode.DependencyPaths);
			MyWaitHandle.WaitOne();
			foreach (var path in deps)
				tasks[path].Wait();

			// Parse the file
			var rs = ResourceReader.OpenResource(File.ReadAllText(GetSourcePath(myNode.Path)), myNode.Path, DependencyWaiter);
			var res = rs.Read();
			if (!rs.Valid)
			{
				res = null;
				Console.WriteLine("Invalid source.");
				Console.ReadLine();
				throw new ApplicationException();
			}
			using (var fs = File.Create(GetObjectPath(myNode.Path)))
			{
				res.Serialize(fs);
			}
		}
	}
}
