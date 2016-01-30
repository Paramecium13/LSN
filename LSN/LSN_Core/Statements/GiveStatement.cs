using System.Collections.Generic;
using System.Linq;
using LSN_Core.Expressions;
using System;

namespace LSN_Core.Statements
{
	[Serializable]
	public abstract class GiveStatement : Statement
	{
		public IExpression Amount;

		public IExpression Reciever;

	}
	[Serializable]
	public class GiveItemStatement : GiveStatement
	{
		public IExpression Id;

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
	}
	[Serializable]
	public class GiveArmorStatement : GiveStatement
	{
		public IExpression Id;

		public GiveArmorStatement(IExpression id, IExpression a)
		{
			Id = id;
			Amount = a;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GivArmorTo(Id.Eval(i), ((IntValue)Amount.Eval(i)).Value, Reciever.Eval(i));
			return InterpretValue.Base;
		}
	}
	[Serializable]
	public class GiveWeaponStatement : GiveStatement
	{
		public IExpression Id;

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
	}
}
