using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using Syroot.BinaryData;

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

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)StatementCode.Jump);
			writer.Write(Target);
		}
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
			if (Condition.Equals(oldExpr))
				Condition = newExpr;
			else
				Condition.Replace(oldExpr, newExpr);
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)StatementCode.ConditionalJump);
			writer.Write(Target);
			Condition.Serialize(writer, resourceSerializer);
		}
	}

}
