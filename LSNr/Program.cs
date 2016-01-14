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

		static int Main(string[] args)
		{
			if (args.Length == 0) //throw new ApplicationException("No file given.");
			{
				Console.WriteLine("No file given.");
				return NO_FILE;
			}
			if (!File.Exists(args[0]))
			{
				Console.WriteLine($"The file {args[0]} could not be found.");
				return FILE_NOT_FOUND;
			}
			string src;
			using (var s = new StringReader(args[0]))
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


	}
}
