using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;

namespace LsnCore
{
	public interface IInterpreter
	{
		/// <summary>
		/// ...
		/// </summary>
		LsnValue ReturnValue { get; set; }

		/// <summary>
		/// The next statement to execute.
		/// </summary>
		int NextStatement { get; set; }

		void RunProcedure(IProcedure procedure);
		void RunProcedure(IProcedure procedure, LsnValue[] args);

		/// <summary>
		/// Get the variable at the provided index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		LsnValue GetVariable(int index);

		/// <summary>
		/// Set the value of the variable at the provided index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		void SetVariable(int index, LsnValue value);

		/// <summary>
		/// Get the unique script object.
		/// </summary>
		/// <param name="typeId"></param>
		/// <returns></returns>
		ScriptObject GetUniqueScriptObject(TypeId typeId);

		void SaveVariables(ushort[] indexes, string saveId);

		void LoadVariables(ushort[] indexes, string saveId);

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="target"></param>
		void GiveGoldTo(int amount, LsnValue target);

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="id"></param>
		/// <param name="amount"></param>
		/// <param name="target"></param>
		void GiveItemTo(LsnValue id, int amount, LsnValue target);

		/// <summary>
		/// Display a message to the player with an optional title and graphic.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="graphic"> The id of the graphic to display, null if no graphic should be displayed.</param>
		/// <param name="title">The title to display, null if no title should be displayed.</param>
		void Say(string message, LsnValue graphic, string title);

		//LsnValue GetGlobalVariable(string globalVarName/*, string fileName*/);

		//void SetGlobalVariable(LsnValue value, string globalVarName/*, string fileName*/);

		//void WatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange);

		//void UnwatchGlobalVariable(string globalVarName/*, string fileName*/, OnGlobalVariableValueChanged onChange);*/

		/// <summary>
		/// Register a choice for the player and the index of the instruction to jump to if the player selects that choice.
		/// </summary>
		/// <param name="text">The text to display</param>
		/// <param name="target">The position in the code corresponding to this choice</param>
		void RegisterChoice(string text, int target);

		/// <summary>
		/// Display the registered choices to the player and return the index of the instruction to jump to.
		/// </summary>
		/// <returns></returns>
		int DisplayChoices();

		/// <summary>
		/// Clear the registered choices.
		/// </summary>
		void ClearChoices();

		/// <summary>
		/// Set the seed used for random number generation.
		/// </summary>
		/// <param name="seed"></param>
		void RngSetSeed(int seed);

		/// <summary>
		/// Get a random integer.
		/// </summary>
		/// <param name="min">The minimum value, inclusive.</param>
		/// <param name="max">The maximum value, exclusive.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
		int RngGetInt(int min, int max);

		/// <summary>
		/// Get a random double from the uniform distribution [0,1).
		/// </summary>
		/// <returns></returns>
		double RngGetDouble();

		/// <summary>
		/// Get a string from the player.
		/// </summary>
		/// <param name="prompt"></param>
		/// <returns></returns>
		string GetString(string prompt);

		/// <summary>
		/// Get an integer from the player.
		/// </summary>
		/// <param name="prompt"></param>
		/// <returns></returns>
		int GetInt(string prompt);

		/// <summary>
		/// Get a double from the player
		/// </summary>
		/// <param name="prompt"></param>
		/// <returns></returns>
		double GetDouble(string prompt);
	}

	public static class InterpreterExtensions
	{
		/// <summary>
		/// Get a random double from the specified uniform distribution.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="min">The minimum value, inclusive.</param>
		/// <param name="max">The maximum value, exclusive.</param>
		/// <returns></returns>
		public static double RngGetDouble(this IInterpreter i, double min, double max)
			=> min + (max - min) * i.RngGetDouble();

		/// <summary>
		/// Returns a random number from the standard normal distribution.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public static double RngGetNormal(this IInterpreter i)
		{
			var u1 = i.RngGetDouble();
			var u2 = i.RngGetDouble();
			return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
		}

		/// <summary>
		/// Returns a random number from the specified normal distribution.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="mean"></param>
		/// <param name="standardDeviation"></param>
		/// <returns></returns>
		public static double RngGetNormal(this IInterpreter i, double mean, double standardDeviation)
			=> mean + i.RngGetNormal() * standardDeviation;

		/// <summary>
		/// Returns a random number from the standard normal distribution that is within 'cap'
		/// standard deviations of the mean.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="cap">The furthest the result can be from 0.</param>
		/// <returns></returns>
		public static double RngGetCappedNormal(this IInterpreter i, double cap)
		{
			var x = i.RngGetNormal();
			while (x > cap || x < -cap)
				x = i.RngGetNormal();
			return x;
		}

		/// <summary>
		/// Returns a random number from the specified normal distribution that is within 'cap'
		/// of the mean.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="mean">The mean value.</param>
		/// <param name="standardDeviation">The standard deviation</param>
		/// <param name="cap">The furthest away the result can be from the mean.</param>
		/// <returns></returns>
		public static double RngGetCappedNormal(this IInterpreter i, double mean, double standardDeviation, double cap)
			=> mean + standardDeviation * i.RngGetCappedNormal(cap / standardDeviation);
	}
}