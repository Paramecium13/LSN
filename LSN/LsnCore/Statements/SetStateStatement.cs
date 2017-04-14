using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Values;

namespace LsnCore.Statements
{
	public sealed class SetStateStatement : Statement
	{
		private readonly int State;

		public SetStateStatement(int state)
		{
			State = state;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			(i.GetValue(0).Value as ScriptObject).SetState(State);
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}
	}
}
