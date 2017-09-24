using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	[Serializable]
	public sealed class ExpressionStatement : Statement
	{
		private IExpression Expression; // I may have to expose this for optimization.

		public ExpressionStatement(IExpression expression)
		{
			Expression = expression;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			Expression.Eval(i);
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Expression.Equals(oldExpr))
				Expression = newExpr;
			else
				Expression.Replace(oldExpr, newExpr);
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)StatementCode.EvaluateExpression);
			Expression.Serialize(writer, resourceSerializer);
		}
	}
}
