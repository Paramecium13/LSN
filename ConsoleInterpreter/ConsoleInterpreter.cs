using LsnCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleInterpreter
{
	class ConsoleInterpreter : Interpreter
	{
		public ConsoleInterpreter()
		{
			ResourceManager = null;
		}

		public static bool BoolChoice(string prompt)
		{
			while (true)
			{
				Console.Write(prompt);
				switch (Console.ReadLine().ToLowerInvariant().Trim())
				{
					case "y":
					case "yes":
						return true;
					case "n":
					case "no":
						return false;
				}
				Console.WriteLine("Invalid response; Expected (y)es or (n)o.");
			}
		}

		public override int DisplayChoices()
		{
			{
				var i = 1;
				foreach (var choice in Choices)
				{
					Console.WriteLine($"({i.ToString()}) {choice.Item1}");
					i++;
				}
			}
			var length = Choices.Count;
			Console.Write("Pick an option from above: ");
			while (true)
			{
				var ch = Console.ReadLine().Trim();
				if(!int.TryParse(ch,out int i) || i < 1 || i > Choices.Count)
				{
					Console.WriteLine("Invalid response.");
					Console.WriteLine($"Please enter an integer between 1 and {Choices.Count}:");
					continue;
				}
				return Choices[i - 1].Item2;
			}
		}

		public override double GetDouble(string prompt)
		{
			while (true)
			{
				if(prompt != null) Console.Write(prompt);
				var ch = Console.ReadLine().Trim();
				if (double.TryParse(ch, out double x))
					return x;
				Console.WriteLine("Invalid response; response should be a number.");
			}
		}

		public override int GetInt(string prompt)
		{
			while (true)
			{
				if (prompt != null) Console.Write(prompt);
				var ch = Console.ReadLine().Trim();
				if (int.TryParse(ch, out int x))
					return x;
				Console.WriteLine("Invalid response; response should be an integer.");
			}
		}

		public override string GetString(string prompt)
		{
			if (prompt != null) Console.Write(prompt);
			return Console.ReadLine();
		}

		public override void GiveGoldTo(int amount, LsnValue target)
		{
			throw new NotImplementedException();
		}

		public override void GiveItemTo(LsnValue id, int amount, LsnValue target)
		{
			throw new NotImplementedException();
		}

		public override void Say(string message, LsnValue graphic, string title)
		{
			if (title != null)
				Console.WriteLine($"{title}: {message}");
			else Console.WriteLine(message);
		}
	}
}
