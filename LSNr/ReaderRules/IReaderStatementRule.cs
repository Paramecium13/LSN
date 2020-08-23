using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Utilities;

namespace LSNr.ReaderRules
{
	/// <summary>
	/// A rule for reading statements, sequences of tokens ending in ;.
	/// </summary>
	public interface IReaderStatementRule
	{
		/// <summary>
		/// Checks if the specified tokens satisfy this rule.
		/// </summary>
		/// <param name="tokens">The tokens, including the terminal ;.</param>
		/// <returns></returns>
		bool Check(ISlice<Token> tokens);

		/// <summary>
		/// Applies this rule using the specified tokens.
		/// </summary>
		/// <param name="tokens">The tokens, including the terminal ;.</param>
		/// <param name="attributes">The attributes.</param>
		void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
	}

	/// <summary>
	/// A rule for reading blocks bounded by { and }.
	/// </summary>
	public interface IReaderBodyRule
	{
		/// <summary>
		/// Checks if the specified tokens satisfy this rule.
		/// </summary>
		/// <param name="tokens"> The tokens. </param>
		/// <returns></returns>
		bool Check(ISlice<Token> head);

		/// <summary>
		/// Applies this rule using the specified tokens.
		/// </summary>
		/// <param name="head"> The head of the body, i.e. the tokens appearing before the opening {. </param>
		/// <param name="body"> The tokens enclosed within the { and }. </param>
		/// <param name="attributes"> The attributes. </param>
		void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
	}
}

