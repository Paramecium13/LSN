using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	internal sealed class PreStatement
	{
		public readonly Statement Statement;
		public string Label;
		public string Target;

		public PreStatement(Statement statement)
		{
			Statement = statement;
		}
	}
}
