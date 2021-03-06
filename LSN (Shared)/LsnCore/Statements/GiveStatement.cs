﻿using System.Collections.Generic;
using System.Linq;
using LsnCore.Expressions;
using System;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public abstract class GiveStatement : Statement
	{
		protected IExpression Amount;

		protected IExpression Receiver;
	}

	public sealed class GiveItemStatement : GiveStatement
	{
		private IExpression Id;

		public GiveItemStatement(IExpression id, IExpression amount, IExpression receiver)
		{
			Id = id; Amount = amount; Receiver = receiver;
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Amount;
			foreach (var expr in Amount.SelectMany(e => e))
				yield return expr;

			if (Receiver != null && ! Receiver.Equals(LsnValue.Nil))
			{
				yield return Receiver;
				foreach (var expr in Receiver.SelectMany(e => e))
					yield return expr;
			}

			yield return Id;
			foreach (var expr in Id)
				yield return expr;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GiveItemTo(Id.Eval(i), Amount.Eval(i).IntValue, Receiver.Eval(i));
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Amount.Equals(oldExpr)) Amount = newExpr;
			if (Receiver.Equals(oldExpr)) Receiver = newExpr;
			if (Id.Equals(oldExpr)) Id = newExpr;
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.GiveItem);
			Amount.Serialize(writer, resourceSerializer);
			Id.Serialize(writer, resourceSerializer);
			Receiver.Serialize(writer, resourceSerializer);
		}
	}

	public sealed class GiveGoldStatement : GiveStatement
	{
		public GiveGoldStatement(IExpression amount, IExpression receiver)
		{
			Amount = amount; Receiver = receiver;
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Amount;
			foreach (var expr in Amount.SelectMany(e => e))
				yield return expr;

			if (Receiver == null || Receiver.Equals(LsnValue.Nil)) yield break;
			yield return Receiver;
			foreach (var expr in Receiver.SelectMany(e => e))
				yield return expr;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			i.GiveGoldTo(Amount.Eval(i).IntValue, Receiver.Eval(i));
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Amount.Equals(oldExpr)) Amount = newExpr;
			if (Receiver.Equals(oldExpr)) Receiver = newExpr;
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.GiveGold);
			Amount.Serialize(writer, resourceSerializer);
			Receiver.Serialize(writer, resourceSerializer);
		}
	}
}
