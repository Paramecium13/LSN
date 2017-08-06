using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LsnCore.Statements
{
	[Serializable]
	public sealed class DisplayChoicesStatement : Statement
	{
		public override InterpretValue Interpret(IInterpreter i)
		{
			i.NextStatement = i.DisplayChoices();
			i.ClearChoices();
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr){}
	}
}
