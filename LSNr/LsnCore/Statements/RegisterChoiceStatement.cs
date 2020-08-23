using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public sealed class RegisterChoiceStatement : Statement, IHasTargetStatement
	{
		internal IExpression Condition;

		internal IExpression ChoiceText;

		public int Target { get; set; } = -1;
#if LSNR
		public string Label { get; }

		internal RegisterChoiceStatement(IExpression condition, IExpression choiceText, string label):this(condition, choiceText)
		{
			Label = label;
		}
#endif
		internal RegisterChoiceStatement(IExpression condition, IExpression choiceText)
		{
			Condition = condition; ChoiceText = choiceText;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			if(Condition.Eval(i).BoolValue)
				i.RegisterChoice((ChoiceText.Eval(i).Value as StringValue).Value, Target);
			return InterpretValue.Base;
		}
#endif

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

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.RegisterChoice);
			writer.Write(Target);
			Condition.Serialize(writer, resourceSerializer);
			ChoiceText.Serialize(writer, resourceSerializer);
		}

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
	}
}
