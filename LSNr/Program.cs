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
using System.Threading.Tasks;

namespace LSNr
{
	static class Program
	{
		private const int NO_FILE = -1;
		private const int FILE_NOT_FOUND = -2;
		private const int ERROR_IN_SOURCE = -3;
		private const int CONFIG_EXISTS = -4;

		private static Config _Config;

		internal static Config Config => _Config;

		static int Main(string[] args)
		{
			if (args.Length == 0 || string.Equals(args[0], "setup", StringComparison.OrdinalIgnoreCase)) // Set up the workspace.
			{
				return SetUp();
			}

			if(File.Exists("lsn.config"))
			{
				using (var strReader = new StringReader(File.ReadAllText("lsn.config")))
					_Config = JsonSerializer.Create().Deserialize<Config>(new JsonTextReader(strReader));
			}
			else
				_Config = new Config();

			if(string.Equals(args[0], "build", StringComparison.OrdinalIgnoreCase))
			{
				return Build(args);
			}
			if (!File.Exists(args[0]))
			{
				Console.WriteLine($"The file {args[0]} could not be found.");
				return FILE_NOT_FOUND;
			}

			LssParser.ExpressionParser.DefaultSetUp();

			string src;
			using (var s = new StreamReader(args[0],Encoding.UTF8))
			{
				src = s.ReadToEnd();
			}
			var destination = GetObjectPath(args[0]);

			// The argument that specifies the destination, or null if not present
			var dest = args.FirstOrDefault(a => Regex.IsMatch(a, @"^\s*destination\s*=.+$", RegexOptions.IgnoreCase));

			if (dest != null)
			{
				destination = Regex.Match(dest, @"^\s*destination\s*=\s*(.+)\s*$", RegexOptions.IgnoreCase).Groups[1].Value;
			}

			// The argument that specifies
			if (Path.IsPathRooted(args[0]))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error: The file to reify must be passed as a relative path.");
				Console.ResetColor();
				return -5;
			}
			LsnResourceThing res = null;
			return MakeResource(new string(args[0].Take(args[0].Length - 4).ToArray()),src, destination, out res, args);
		}

		internal static int MakeResource(string path, string src, string destination, out LsnResourceThing res, string[] args)
		{
			var rs = new PreResource(src,path);
			rs.Reify();
			if(! rs.Valid)
			{
				res = null;
				Console.WriteLine("Invalid source.");
				Console.ReadLine();
				return ERROR_IN_SOURCE;
			}
			res = rs.GetResource();
			using (var fs = File.Create(destination))
			{
				res.Serialize(fs);
			}
			return 0;
		}

		private static int SetUp()
		{
			const string file = "lsn.config";
            if (File.Exists(file)) return CONFIG_EXISTS;
			var config = new Config();

			using (var f = File.Create(file))
			{
				using (var writer = new StreamWriter(f))
				{
					writer.Write(/*COMMENTS + END_OF_COMMENTS + */JsonConvert.SerializeObject(config, Formatting.Indented));
				}
			}
			Directory.CreateDirectory("src");
			Directory.CreateDirectory("obj");

			Directory.CreateDirectory(@"src\script");
			Directory.CreateDirectory(@"src\scriptlet");
			Directory.CreateDirectory(@"src\resource");
			Directory.CreateDirectory(@"src\scene");
			Directory.CreateDirectory(@"src\quest");

			Directory.CreateDirectory(@"obj\script");
			Directory.CreateDirectory(@"obj\scriptlet");
			Directory.CreateDirectory(@"obj\resource");
			Directory.CreateDirectory(@"obj\scene");
			Directory.CreateDirectory(@"obj\quest");
			return 0;
		}

		private static int Build(string[] args)
		{
			throw new NotImplementedException();
			if(args.Length > 1 && args[1].ToLower() == "all")
			{

			}
			else // Only build the files that aren't up to date.
			{

			}
			return 0;
		}

		public static string GetObjectPath(string rawPath)
		{
			if(rawPath.StartsWith("obj"))
			{
				if (Path.HasExtension(rawPath))
					return rawPath;
				return rawPath + Config.ObjectFileExtension;
			}
			if(rawPath.StartsWith("src"))
			{
				rawPath = new string(rawPath.Skip(4).ToArray());
			}
			if(Path.HasExtension(rawPath))
			{
				if(Path.GetExtension(rawPath) != Config.ObjectFileExtension)
				{
					rawPath = new string(rawPath.Take(rawPath.Length - Path.GetExtension(rawPath).Length).Concat(Config.ObjectFileExtension).ToArray());
				}
				return Path.Combine("obj",rawPath);
			}

			return Path.Combine("obj", rawPath + Config.ObjectFileExtension);
		}

		public static string GetSourcePath(string rawPath)
		{
			return Path.Combine("src", rawPath + ".lsn");
		}
	}
}
