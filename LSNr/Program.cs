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
	class Program
	{
		private const int NO_FILE = -1;
		private const int FILE_NOT_FOUND = -2;
		private const int ERROR_IN_SOURCE = -3;
		private const int CONFIG_EXISTS = -4;

		private const string END_OF_COMMENTS = "⌧";
		private const string COMMENTS = "~~~Remarks Section~~~\nDescribe your workplace here\n\n\n~~~Do comment below this line~~~\n";

		private static Config _Config;


		internal static Config Config => _Config;


		static int Main(string[] args)
		{
			if (args.Length == 0 || args[0].ToLower() == "setup") // Set up the workspace.
			{
				return SetUp();
			}

			if(File.Exists("lsn.config"))
			{
				_Config = JsonSerializer.Create()
					.Deserialize<Config>(new JsonTextReader(new StringReader(File.ReadAllText("lsn.config"))));
			}
			else
				_Config = new Config();
			

			if(args[0].ToLower() == "build")
			{
				return Build(args);
			}
			if (!File.Exists(args[0]))
			{
				Console.WriteLine($"The file {args[0]} could not be found.");
				return FILE_NOT_FOUND;
			}
			string src;
			using (var s = new StreamReader(args[0],Encoding.UTF8))
			{
				src = s.ReadToEnd();
			}
			string destination = GetObjectPath(args[0]);
			
			// The argument that specifies the destination, or null if not present
			string dest = args.Where(a => Regex.IsMatch(a, @"^\s*destination\s*=.+$", RegexOptions.IgnoreCase))
				.FirstOrDefault();
			if (dest != null)
				destination = dest;//Regex.Match(dest, @"^\s*destination\s*=\s*(.+)\s*$", RegexOptions.IgnoreCase).Captures[0].ToString();

			// The argument that specifies
			string t = args.Where(a => Regex.IsMatch(a,@"^\s*type\s*=\s*(\w+)\s*$",RegexOptions.IgnoreCase)).FirstOrDefault();
			if (t != null)
			{ // The type of file is defined.
				string type = Regex.Match(t, @"^\s*type\s*=\s*(\w+)\s*$", RegexOptions.IgnoreCase)
					.Groups.Cast<Group>()
					.Select(g => g.Value)
					.ToArray()[1];
				if (type == "resource" || type == "res")
				{
					LsnResourceThing res = null;
					return MakeResource(src, destination, out res, args);
				}

				if (type == "quest") throw new NotImplementedException();
			}
			// Otherwise it's a script.
			return MakeScript(src,destination,args);
		}

		
		
		private static int MakeScript(string src, string destination, string[] args)
		{
			var sc = new PreScript(src);
			sc.Reify();
			if(! sc.Valid)
			{
				Console.WriteLine("Invalid source.");
				// Write error messages, if not already done. (Errors should be printed during reification)
				return ERROR_IN_SOURCE;
			}
			var res = sc.GetScript();
			using (var fs = new FileStream(destination, FileMode.Create))
			{
				new BinaryFormatter().Serialize(fs, res);
			}
			return 0;
		}



		internal static int MakeResource(string src, string destination, out LsnResourceThing res, string[] args)
		{
			var rs = new PreResource(src);
			rs.Reify();
			if(! rs.Valid)
			{
				res = null;
				Console.WriteLine("Invalid source.");
				// Write error messages, if not already done. (Errors should be printed during reification)
				return ERROR_IN_SOURCE;
			}
			res = rs.GetResource();
			using (var fs = new FileStream(destination, FileMode.Create))
			{
				new BinaryFormatter().Serialize(fs, res);
			}
			return 0;
		}


		private static int SetUp()
		{
			const string file = "lsn.config";
            if (File.Exists(file)) return CONFIG_EXISTS;
			var config = new Config();
			using (var writer = new StreamWriter(File.Create(file)))
			{
				writer.Write(/*COMMENTS + END_OF_COMMENTS + */JsonConvert.SerializeObject(config, Formatting.Indented));
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
