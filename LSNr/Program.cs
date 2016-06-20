﻿using Newtonsoft.Json;
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
		private const string COMMENTS = "~~~Remarks Section~~~\n\n\n\n~~~Do not edit below this line~~~\n";

		static int Main(string[] args)
		{
			var tem = LsnCore.LsnType.int_;
			if (args.Length == 0 || args[0].ToLower() == "setup") // Set up the workspace.
			{
				return SetUp();
			}
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
			string destination = args[0].Replace(".lsn", ".dat");
			
			// The argument that specifies the destination, or null if not present
			string dest = args.Where(a => Regex.IsMatch(a, @"^\s*destination\s*=.+$", RegexOptions.IgnoreCase))
				.FirstOrDefault();
			if (dest != null)
				destination = Regex.Match(dest, @"^\s*destination\s*=\s*(.+)\s*$", RegexOptions.IgnoreCase).Captures[0].ToString();

			// The argument that specifies
			string t = args.Where(a => Regex.IsMatch(a,@"^\s*type\s*=\s*\w+\s*$",RegexOptions.IgnoreCase)).FirstOrDefault();
			if (t != null)
			{ // The type of file is defined.
				string type = Regex.Match(t, @"^\s*type\s*=\s*(\w+)\s*$", RegexOptions.IgnoreCase).Captures[0].ToString().ToLower();
				if (type == "resource" || type == "res") return MakeResource(src, destination, args);

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
			var x = new BinaryFormatter();
			x.Serialize(new FileStream(args[0].Replace(".lsn", ".dat"), FileMode.Create), res);
			return 0;
		}



		private static int MakeResource(string src, string destination, string[] args)
		{
			return -10;
		}


		private static int SetUp()
		{
			const string file = "lsn.config";
            if (File.Exists(file)) return CONFIG_EXISTS;
			var config = new Config();
			using (var writer = new StreamWriter(File.Create(file)))
			{
				writer.Write(COMMENTS + END_OF_COMMENTS + JsonConvert.SerializeObject(config, Formatting.Indented));
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

	}
}
