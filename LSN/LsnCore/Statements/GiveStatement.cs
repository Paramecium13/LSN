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

		protected IExpression Reciever;

	}
	[Serializable]
	public class GiveItemStatement : GiveStatement
	{
		private IExpression Id;

		public GiveItemStatement(IExpression id, IExpression a)
		{
			Id = id;
			Amount = a;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GiveItemTo(Id.Eval(i), ((IntValue)Amount.Eval(i)).Value, Reciever.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Amount.Equals(oldExpr)) Amount = newExpr;
			if (Reciever.Equals(oldExpr)) Reciever = newExpr;
			if (Id.Equals(oldExpr)) Id = newExpr;
		}
	}
	[Serializable]
	public class GiveArmorStatement : GiveStatement
	{
		private IExpression Id;

		public GiveArmorStatement(IExpression id, IExpression a)
		{
			Id = id;
			Amount = a;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GiveArmorTo(Id.Eval(i), ((IntValue)Amount.Eval(i)).Value, Reciever.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Amount.Equals(oldExpr)) Amount = newExpr;
			if (Reciever.Equals(oldExpr)) Reciever = newExpr;
			if (Id.Equals(oldExpr)) Id = newExpr;
		}
	}
	[Serializable]
	public class GiveWeaponStatement : GiveStatement
	{
		private IExpression Id;

		public GiveWeaponStatement(IExpression id, IExpression a)
		{
			Id = id;
			Amount = a;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GiveWeaponTo(Id.Eval(i), ((IntValue)Amount.Eval(i)).Value, Reciever.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Amount.Equals(oldExpr)) Amount = newExpr;
			if (Reciever.Equals(oldExpr)) Reciever = newExpr;
			if (Id.Equals(oldExpr)) Id = newExpr;
		}
	}
	[Serializable]
	public class GiveGoldStatement : GiveStatement
	{
		public GiveGoldStatement(IExpression a)
		{
			Amount = a;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GiveGoldTo(((IntValue)Amount.Eval(i)).Value,Reciever.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Amount.Equals(oldExpr)) Amount = newExpr;
			if (Reciever.Equals(oldExpr)) Reciever = newExpr;
		}
	}
}
