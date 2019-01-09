using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Utilities;

namespace LSNr.ReaderRules
{
	public interface IReaderStatementRule
	{
		bool Check(ISlice<Token> tokens);

		void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
	}

	public interface IReaderBodyRule
	{
		bool Check(ISlice<Token> head);

		void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
	}
}

