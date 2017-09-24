using System.Collections.Generic;
using System.Linq;
using LsnCore.Expressions;
using System;
using Syroot.BinaryData;

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

		public GiveItemStatement(IExpression id, IExpression amount, IExpression receiver)
		{
			Id = id; Amount = amount; Receiver = receiver;
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

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)StatementCode.GiveItem);
			Amount.Serialize(writer, resourceSerializer);
			Id.Serialize(writer, resourceSerializer);
			Receiver.Serialize(writer, resourceSerializer);
		}
	}
	
	[Serializable]
	public class GiveGoldStatement : GiveStatement
	{
		public GiveGoldStatement(IExpression amount, IExpression receiver)
		{
			Amount = amount; Receiver = receiver;
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

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)StatementCode.GiveGold);
			Amount.Serialize(writer, resourceSerializer);
			Receiver.Serialize(writer, resourceSerializer);
		}
	}
}
