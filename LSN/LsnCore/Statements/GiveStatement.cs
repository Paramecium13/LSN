using System.Collections.Generic;
using System.Linq;
using LsnCore.Expressions;
using System;

namespace LsnCore.Statements
{
	[Serializable]
	public abstract class GiveStatement : Statement
	{
		protected IExpression Amount;

		protected IExpression Receiver;
		
	}
	[Serializable]
	public class GiveItemStatement : GiveStatement
	{
		private IExpression Id;

		public GiveItemStatement(IExpression id, IExpression a, IExpression receiver)
		{
			Id = id; Amount = a; Receiver = receiver;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GiveItemTo(Id.Eval(i), Amount.Eval(i).IntValue, Receiver.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Amount.Equals(oldExpr)) Amount = newExpr;
			if (Receiver.Equals(oldExpr)) Receiver = newExpr;
			if (Id.Equals(oldExpr)) Id = newExpr;
		}
	}
	
	[Serializable]
	public class GiveGoldStatement : GiveStatement
	{
		public GiveGoldStatement(IExpression a, IExpression receiver)
		{
			Amount = a; Receiver = receiver;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GiveGoldTo(Amount.Eval(i).IntValue, Receiver.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Amount.Equals(oldExpr)) Amount = newExpr;
			if (Receiver.Equals(oldExpr)) Receiver = newExpr;
		}
	}
}
