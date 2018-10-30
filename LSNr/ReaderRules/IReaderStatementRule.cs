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
		bool Check(ISearchableReadOnlyList<Token> tokens);

		void Apply(ISearchableReadOnlyList<Token> tokens);
	}

	public interface IReaderBodyRule
	{
		bool Check(ISearchableReadOnlyList<Token> head);

		void Apply(ISearchableReadOnlyList<Token> head, ISearchableReadOnlyList<Token> body);
	}
}

