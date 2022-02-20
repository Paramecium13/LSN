using LsnCore;
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
	internal static class Program
	{
		private const int NO_FILE = -1;
		private const int FILE_NOT_FOUND = -2;
		private const int ERROR_IN_SOURCE = -3;
		private const int CONFIG_EXISTS = -4;
		private const string DEP_FILE_PATH = "dependencies.json";

		private static MainFile _MainFile;

		internal static MainFile MainFile => _MainFile;
		private static readonly EventWaitHandle MyWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
		private static DependenciesFile _DependenciesFile;

		private static int Main(string[] args)
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
			var deps =  _DependenciesFile.RegisterChangedFiles(changedFiles);
			var tasks = new Dictionary<string, Task>();
			foreach (var path in changedFiles.Union(deps).Where(s => !string.IsNullOrEmpty(s))) // deps may contain empty strings for some reason...
				tasks[new string(path.Skip(4).Take(path.Length - 8).ToArray())] = Task.Run(() => Reify(path));
			DependencyWaiter = new DependencyWaiter(_DependenciesFile, tasks);
			MyWaitHandle.Set();

			var rTask = Task.WhenAll(tasks.Values);

			try
			{
				rTask.Wait();
			}
			catch(Exception)
			{
				return 8;
			}
			finally
			{
				if(tasks.Values.Any(t => t.Status == TaskStatus.RanToCompletion))
					_DependenciesFile.Write(DEP_FILE_PATH);
			}

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
					if (srcUpdateTime >= objUpdateTime)
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
			if (rawPath.StartsWith("obj"))
			{
				if (Path.HasExtension(rawPath))
					return rawPath;
				return rawPath + _MainFile.ObjectFileExtension;
			}
			if (rawPath.StartsWith("src"))
			{
				rawPath = new string(rawPath.Skip(4).ToArray());
			}

			if (!Path.HasExtension(rawPath)) return Path.Combine("obj", rawPath + _MainFile.ObjectFileExtension);
			if (Path.GetExtension(rawPath) != _MainFile.ObjectFileExtension)
			{
				rawPath = new string(rawPath.Take(rawPath.Length - Path.GetExtension(rawPath).Length).Concat(_MainFile.ObjectFileExtension).ToArray());
			}
			return Path.Combine("obj", rawPath);

		}

		public static string GetSourcePath(string rawPath)
		{
			if (!rawPath.EndsWith(".lsn"))
				rawPath += ".lsn";
			if (!rawPath.StartsWith("src\\"))
				rawPath = Path.Combine("src", rawPath);
			return rawPath;
		}

		private static DependencyWaiter DependencyWaiter;

		public static void Reify(string path)
		{
			MyWaitHandle.WaitOne();

			// Parse the file
			var rs = ResourceReader.OpenResource(File.ReadAllText(GetSourcePath(path)), path);
			var res = rs.Read();
			if (!rs.Valid)
			{
				res = null;
				Console.WriteLine("Invalid source.");
				Console.ReadLine();
				throw new ApplicationException();
			}

			using var fs = File.Create(GetObjectPath(path));
			res.Serialize(fs);
		}

		internal static LsnResourceThing Load(string user, string used)
		{
			if (used.StartsWith(@"Lsn Core\", StringComparison.Ordinal))
				return ResourceManager.GetStandardLibraryResource(new string(used.Skip(9).ToArray()));
			if (used.StartsWith(@"std\", StringComparison.Ordinal))
				return ResourceManager.GetStandardLibraryResource(new string(used.Skip(4).ToArray()));
			DependencyWaiter.WaitOn(user, used);
			LsnResourceThing res = null;
			var objPath = Program.GetObjectPath(used);
			using var fs = File.OpenRead(objPath);
			res = LsnResourceThing.Read(fs, used, (x) => Load(user,x));
			return res;
		}
	}
}
