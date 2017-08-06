using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LsnCore.Statements
{
	internal interface IHasTargetStatement
	{
		int Target { get; set; }
	}

	[Serializable]
	public sealed class JumpStatement : Statement, IHasTargetStatement
	{
		public int Target { get; set; } = -1;



		public override InterpretValue Interpret(IInterpreter i)
		{
			i.NextStatement = Target;
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr){}
	}

	[Serializable]
	public sealed class ConditionalJumpStatement : Statement, IHasTargetStatement
	{
		internal IExpression Condition;
		public int Target { get; set; } = -1;

		internal ConditionalJumpStatement(IExpression condition)
		{
			Condition = condition;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			if (Condition.Eval(i).BoolValue)
				i.NextStatement = Target;
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			throw new NotImplementedException();
		}
	}

}
