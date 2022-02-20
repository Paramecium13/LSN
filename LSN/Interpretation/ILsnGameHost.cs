using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	public interface ILsnGameHost
	{
		/// <summary>
		/// Hello World!
		/// 
		/// Doesn't return until the message is 'done'...
		/// </summary>
		/// <param name="message"></param>
		/// <param name="title">Who (or what) is saying this.</param>
		/// <param name="with"></param>
		void Say(string message, LsnValue with, string title);

		void RegisterChoice(string choice, int target);

		/// <summary>
		/// Displays all the registered choices to the player
		/// and returns the number associated with the chosen choice.
		/// </summary>
		/// <returns>The number associated with the chosen choice</returns>
		int DisplayChoices();

		/// <summary>
		/// Clears the registered choices.
		/// </summary>
		void ClearChoices();

		/// <summary>
		/// TBD.
		/// </summary>
		/// <param name="args"></param>
		void GoTo(params LsnValue[] args);

		/// <summary>
		/// TBD.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		LsnValue ComeFrom(params LsnValue[] args);

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
}
