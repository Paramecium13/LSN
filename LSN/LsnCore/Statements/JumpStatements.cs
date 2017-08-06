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
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr){}
	}

	[Serializable]
	public sealed class ConditionalJumpStatement : Statement, IHasTargetStatement
	{
		bool JumpIfTrue;
		internal IExpression Condition;
		public int Target { get; set; } = -1;

		internal ConditionalJumpStatement(IExpression condition, bool jumpIfTrue = false)
		{
			Condition = condition; JumpIfTrue = jumpIfTrue;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			if (Condition.Eval(i).BoolValue == JumpIfTrue)
			{

			}
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			throw new NotImplementedException();
		}
	}

}
