using System;
using System.Collections.Generic;
using LsnCore.Expressions;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public class JumpToTargetStatement : Statement
	{
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr) {}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}
	}
}