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
		/// <summary>
		/// Gets or sets the index of the statement that this statement targets.
		/// </summary>
		int Target { get; set; }
	}

	public sealed class JumpStatement : Statement, IHasTargetStatement
	{
		/// <inheritdoc />
		public int Target { get; set; } = -1;
#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			i.NextStatement = Target;
			return InterpretValue.Base;
		}
#endif

		/// <inheritdoc />
		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		/// <inheritdoc />
		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.Jump);
			writer.Write(Target);
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			instructionList.AddInstruction(new TargetedPreInstruction(OpCode.Jump, target, context.LabelFactory));
		}
	}

	[Serializable]
	public sealed class ConditionalJumpStatement : Statement, IHasTargetStatement
	{
		/// <summary>
		/// The condition of this conditional jump
		/// </summary>
		internal IExpression Condition;

		/// <inheritdoc />
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

		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
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

		/// <inheritdoc/>
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			var targetLabel = context.LabelFactory.GetLabel(target);
			var nextLabel = context.LabelFactory.CreateLabel();
			if (Condition is NotExpression not)
			{
				var subContext = context.WithContext(ExpressionContext.JumpFalseStatement);
				subContext.WantsBoolReturnValue = false;
				subContext.ShortCircuitLabelA = targetLabel;
				subContext.ShortCircuitLabelB = nextLabel;
				not.Value.GetInstructions(instructionList, subContext);
				instructionList.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False, targetLabel));
			}
			else
			{
				var subContext = context.WithContext(ExpressionContext.JumpTrueStatement);
				subContext.WantsBoolReturnValue = false;
				subContext.ShortCircuitLabelA = targetLabel;
				subContext.ShortCircuitLabelB = nextLabel;
				Condition.GetInstructions(instructionList, subContext);
				instructionList.AddInstruction(new TargetedPreInstruction(OpCode.Jump_True, targetLabel));
			}

			instructionList.SetNextLabel(nextLabel);
		}
	}

}
