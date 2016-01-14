using LSN_Core;
using LSN_Core.Expressions;
using LSN_Core.Statements;
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

		public IExpression InitialValue { get; protected set; }

		public List<IExpression> SubsequentValues { get; private set; } = new List<IExpression>();

		public List<IExpression> Users { get; set; } = new List<IExpression>();

		public AssignmentStatement Assignment { get; private set; }

		public bool Used { get { return Users.Count > 0; } }

		public Variable(string name, bool m, IExpression init, AssignmentStatement assignment)
		{
			Name = name;
			Type = init.Type;
			Mutable = m;
			InitialValue = init;
			Assignment = assignment;
		}

		public bool Const()
		{
			return (!Mutable && InitialValue.IsReifyTimeConst());
		}

		/// <summary>
		/// Performs type checking...
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool AddValue(IExpression value)
		{
			if(Type.Subsumes(value.Type))
			{
				Users.Add(value);
				return true;
			}
			return false;
		}

	}
}
