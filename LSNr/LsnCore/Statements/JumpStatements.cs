using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	internal interface IHasTargetStatement
	{
		int Target { get; set; }
	}

	public sealed class JumpStatement : Statement, IHasTargetStatement
	{
		public int Target { get; set; } = -1;
#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			i.NextStatement = Target;
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.Jump);
			writer.Write(Target);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			instructionList.AddInstruction(new TargetedPreInstruction(OpCode.Jump, target, context.LabelFactory));
		}

		/// <inheritdoc />
		protected override IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context)
		{
			yield return new TargetedPreInstruction(OpCode.Jump, target, context.LabelFactory);
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

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			if (Condition.Eval(i).BoolValue)
				i.NextStatement = Target;
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr))
				Condition = newExpr;
			else
				Condition.Replace(oldExpr, newExpr);
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.ConditionalJump);
			writer.Write(Target);
			Condition.Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Condition;
			foreach (var expr in Condition.SelectMany(e => e))
				yield return expr;
		}

		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			if (Condition is NotExpression not)
			{
				not.Value.GetInstructions(instructionList, context.WithContext(ExpressionContext.SubExpression));
				instructionList.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False, target,
					context.LabelFactory));
			}
			else
			{
				Condition.GetInstructions(instructionList, context.WithContext(ExpressionContext.SubExpression));
				instructionList.AddInstruction(new TargetedPreInstruction(OpCode.Jump_True, target,
					context.LabelFactory));
			}
		}

		protected override IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}
	}

}
