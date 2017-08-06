using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LsnCore.Statements
{
	[Serializable]
	public sealed class RegisterChoiceStatement : Statement, IHasTargetStatement
	{
		internal IExpression Condition;

		internal IExpression ChoiceText;

		public int Target { get; set; } = -1;

		internal RegisterChoiceStatement(IExpression condition, IExpression choiceText)
		{
			Condition = condition; ChoiceText = choiceText;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			if(Condition.Eval(i).BoolValue)
				i.RegisterChoice((ChoiceText.Eval(i).Value as StringValue).Value, Target);
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			throw new NotImplementedException();
		}
	}
}
