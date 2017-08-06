using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LsnCore.Statements
{
	/// <summary>
	/// Exit the innermost loop.
	/// </summary>
	//[Serializable]
	public class BreakStatement : Statement
	{
		public override InterpretValue Interpret(IInterpreter i) => InterpretValue.Break;

		public override void Replace(IExpression oldExpr, IExpression newExpr){}
	}

	/// <summary>
	/// Move on to the next iteration of the innermost loop.
	/// </summary>
	//[Serializable]
	public class NextStatement : Statement
	{
		public override InterpretValue Interpret(IInterpreter i) => InterpretValue.Next;

		public override void Replace(IExpression oldExpr, IExpression newExpr){}
	}

	[Serializable]
	public sealed class CallChoicesStatement : Statement
	{
		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr){}
	}

}
