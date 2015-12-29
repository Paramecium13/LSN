using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0) throw new ApplicationException("No file given.");
			string src;
			using (var s = new StringReader(args[0]))
			{
				src = s.ReadToEnd();
			}
			var sc = new PreScript(src);
			var res = sc.Reify();
			//var f = new FileInfo(args[0]);
			var x = new BinaryFormatter();
			x.Serialize(new FileStream(args[0].Replace(".lsn", ".dat"), FileMode.Create), res);
		}
	}
}
