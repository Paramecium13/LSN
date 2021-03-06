﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using LsnCore.Expressions;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public sealed class CollectionValueAssignmentStatement : Statement
	{
		private IExpression Collection;
		private IExpression Index;
		private IExpression Value;

		public CollectionValueAssignmentStatement(IExpression collection, IExpression index, IExpression value)
		{
			Collection = collection; Index = index; Value = value;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			(Collection.Eval(i).Value as IWritableCollectionValue).SetValue(Index.Eval(i).IntValue, Value.Eval(i));
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Collection.Equals(oldExpr))
				Collection = newExpr;
			else if (Index.Equals(oldExpr))
				Index = newExpr;
			else if (Value.Equals(oldExpr))
				Value = newExpr;
			else
			{
				Collection.Replace(oldExpr, newExpr);
				Index.Replace(oldExpr, newExpr);
				Value.Replace(oldExpr, newExpr);
			}
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.AssignValueInCollection);
			Collection.Serialize(writer, resourceSerializer);
			Index.Serialize(writer, resourceSerializer);
			Value.Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Collection;
			foreach (var expr in Collection.SelectMany(e => e))
				yield return expr;

			yield return Index;
			foreach (var expr in Index.SelectMany(e => e))
				yield return expr;

			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}
	}

	// Make const versions, where Index and/or Value are constant.
}
