﻿using LsnCore.Statements;
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
		public IList<string> Label { get; set; }// = new List<string>();
		public string Target { get; set; }

		/*private List<string> _Labels = new List<string>();
		public IReadOnlyList<string> Labels { get; set; }*/

		public PreStatement(Statement statement)
		{
			Statement = statement;
		}
	}
}
