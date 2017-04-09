using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using LsnCore.Expressions;

namespace LsnCore.Statements
{
	[Serializable]
	public sealed class CollectionValueReassignmentStatement : Statement
	{
		private IExpression Collection;
		private IExpression Index;
		private IExpression Value;


		public CollectionValueReassignmentStatement(IExpression collection, IExpression index, IExpression value)
		{
			Collection = collection; Index = index; Value = value;
		}


		public override InterpretValue Interpret(IInterpreter i)
		{
			(Collection.Eval(i).Value as IWritableCollectionValue).SetValue(Index.Eval(i).IntValue, Value.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Collection == oldExpr)
				Collection = newExpr;
			else if (Index == oldExpr)
				Index = newExpr;
			else if (Value == oldExpr)
				Value = newExpr;
			else
			{
				Collection.Replace(oldExpr, newExpr);
				Index.Replace(oldExpr, newExpr);
				Value.Replace(oldExpr, newExpr);
			}
		}
	}

	// Make const versions, where Index and/or Value are LsnValues.
}
