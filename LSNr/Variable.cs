using LSN_Core;
using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class Variable
	{
		public readonly bool Mutable;
		public string Name;
		public readonly LSN_Type Type;

		public IExpression InitialValue { get; set; }

		public Variable(string name, LSN_Type t, bool m, IExpression init)
		{
			Name = name;
			Type = t;
			Mutable = m;
			InitialValue = init;
		}

		public bool Const()
		{
			return (!Mutable && InitialValue.IsReifyTimeConst());
		}

	}
}
