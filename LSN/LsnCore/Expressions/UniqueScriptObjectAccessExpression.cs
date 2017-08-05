using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public sealed class UniqueScriptObjectAccessExpression : Expression
	{
		private readonly string Name;

		public override bool IsPure => false;

		public UniqueScriptObjectAccessExpression(string name, TypeId type)
		{
			Name = name; Type = type;
		}


		public override LsnValue Eval(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

	}
}
