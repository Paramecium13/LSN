using LsnCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleInterpreter
{
	static class Program
	{
		const string ObjDirectory = "obj";

		static ConsoleResourceManager ResourceManager;

		static ConsoleInterpreter Interpreter;

		static int Main(string[] args)
		{
			if(args.Length != 3 /*|| File.Exists(args[0])*/)
			{
				Console.WriteLine("Usage: LSNx <mainFile> <Code file> <function name>.");
				return 4;
			}
			var path = Path.GetFullPath(args[0]);
			var i = path.LastIndexOf(Path.DirectorySeparatorChar);
			Environment.CurrentDirectory = path.Substring(0, i);
			ResourceManager = new ConsoleResourceManager(Path.Combine(Environment.CurrentDirectory, ObjDirectory));
			Interpreter = new ConsoleInterpreter(ResourceManager);

			var res = ResourceManager.GetResource(args[1]);

			var fn = res.Functions[args[2]] as LsnFunction;

			Interpreter.RunProcedure(fn);

			Console.ReadLine();

			return 0;
		}
	}
}
