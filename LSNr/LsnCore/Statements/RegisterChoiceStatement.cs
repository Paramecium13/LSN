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
	public sealed class RegisterChoiceStatement : Statement, IHasTargetStatement
	{
		internal IExpression Condition;

		internal IExpression ChoiceText;

		/// <inheritdoc/>
		public int Target { get; set; } = -1;

		public string Label { get; }

		internal RegisterChoiceStatement(IExpression condition, IExpression choiceText, string label)
		{
			Condition = condition; 
			ChoiceText = choiceText;
			Label = label;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			if(Condition.Eval(i).BoolValue)
				i.RegisterChoice((ChoiceText.Eval(i).Value as StringValue).Value, Target);
			return InterpretValue.Base;
		}
#endif

		/// <inheritdoc/>
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr))
				Condition = newExpr;
			else if (ChoiceText.Equals(oldExpr))
				ChoiceText = newExpr;
			else
			{
				Condition.Replace(oldExpr, newExpr);
				ChoiceText.Replace(oldExpr, newExpr);
			}
		}

		/// <inheritdoc/>
		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.RegisterChoice);
			writer.Write(Target);
			Condition.Serialize(writer, resourceSerializer);
			ChoiceText.Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc/>
		public override IEnumerator<IExpression> GetEnumerator()
		{
			if(!Condition?.Equals(LsnValue.Nil) ?? false)
			{
				yield return Condition;
				foreach (var expr in Condition.SelectMany(e => e))
					yield return expr;
			}

			yield return ChoiceText;
			foreach (var expr in ChoiceText.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc/>
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			if (Condition is LsnValue {BoolValue: false})
			{
				return;
			}

			InstructionLabel registerCndLabel = null;
			InstructionLabel endLabel = null;
			if (Condition != null && Condition is not LsnValue {BoolValue: true})
			{
				registerCndLabel = context.LabelFactory.CreateLabel();
				endLabel = context.LabelFactory.CreateLabel();

				
				var subContext = context.WithContext(ExpressionContext.JumpFalseStatement);
				subContext.ShortCircuitLabelA = endLabel;
				subContext.ShortCircuitLabelB = registerCndLabel;

				Condition.GetInstructions(instructionList, subContext);

				instructionList.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False, endLabel));
			}
			
			if (registerCndLabel != null)
			{
				instructionList.SetNextLabel(registerCndLabel);
			}

			ChoiceText.GetInstructions(instructionList, context.WithContext(ExpressionContext.Internal));
			instructionList.AddInstruction(new TargetedPreInstruction(OpCode.RegisterChoice, target, context.LabelFactory));
			
			if (endLabel != null)
			{
				instructionList.SetNextLabel(endLabel);
			}
		}
	}
}
