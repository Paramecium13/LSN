using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Values;
using LsnCore.Types;

namespace LsnCore.Statements
{
	[Serializable]
	public sealed class FieldAssignmentStatement : Statement
	{
		private IExpression FieldedValue;

		private readonly int Index;

		private IExpression ValueToAssign;


		public FieldAssignmentStatement(IExpression fieldedValue,int index,IExpression value)
		{
			FieldedValue = fieldedValue; Index = index; ValueToAssign = value;
		}


		public override InterpretValue Interpret(IInterpreter i)
		{
			(FieldedValue.Eval(i).Value as IHasMutableFieldsValue).SetFieldValue(Index, ValueToAssign.Eval(i));
			return InterpretValue.Base;
		}


		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (FieldedValue == oldExpr)
				FieldedValue = newExpr;
			else if (ValueToAssign == oldExpr)
				ValueToAssign = newExpr;
			else
			{
				FieldedValue.Replace(oldExpr, newExpr);
				ValueToAssign.Replace(oldExpr, newExpr);
			}
		}
	}

	// Make a const version, where the ValueToAssign is a(n) LsnValue?
}
