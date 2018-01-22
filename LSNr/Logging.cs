using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	static class Logging
	{
		private const ConsoleColor ErrorColor = ConsoleColor.Red;
		internal static void Log(LsnrException exception)
		{
			Console.ForegroundColor = ErrorColor;
			Console.WriteLine($"Error in file '{exception.File}':");
			Console.WriteLine("\t" + exception.Message.Replace("\n", "\n\t"));
			Console.ResetColor();
			Console.WriteLine();
		}

		internal static void Log(string constructType, string constructName, LsnrException exception)
		{
			Console.ForegroundColor = ErrorColor;
			Console.WriteLine($"Error parsing {constructType} '{constructName}' in file '{exception.File}':");
			Console.WriteLine("\t" + exception.Message.Replace("\n", "\n\t"));
			Console.ResetColor();
			Console.WriteLine();
		}

		internal static void Log(string context, LsnrException exception)
		{
			Console.ForegroundColor = ErrorColor;
			Console.WriteLine($"Error parsing {context} in file '{exception.File}':");
			Console.WriteLine("\t" + exception.Message.Replace("\n", "\n\t"));
			Console.ResetColor();
			Console.WriteLine();
		}

		internal static void Log(Exception e, string filePath)
		{
			Console.ForegroundColor = ErrorColor;
			Console.WriteLine($"Unspecified error in file '{filePath}'.");
			Console.WriteLine("This may have been caused by a previously logged error.");
			Console.WriteLine("\t" + e.Message.Replace("\n", "\n\t"));
			Console.ResetColor();
			Console.WriteLine();
		}

		internal static void Log(string context, Exception e, string filePath)
		{
			Console.ForegroundColor = ErrorColor;
			Console.WriteLine($"Unspecified error parsing {context} in file '{filePath}'.");
			Console.WriteLine("This may have been caused by a previously logged error.");
			Console.WriteLine("\t" + e.Message.Replace("\n", "\n\t"));
			Console.ResetColor();
			Console.WriteLine();
		}

		internal static void Log(string constructType, string constructName, Exception e, string filePath)
		{
			Console.ForegroundColor = ErrorColor;
			Console.WriteLine($"Unspecified error parsing {constructType} '{constructName}' in file '{filePath}'.");
			Console.WriteLine("This may have been caused by a previously logged error.");
			Console.WriteLine("\t" + e.Message.Replace("\n", "\n\t"));
			Console.ResetColor();
			Console.WriteLine();
		}
	}
}
