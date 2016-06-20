using System;

namespace LsnCore.Expressions
{
	public interface IExpression
	{
		LsnType Type { get; }

		ILsnValue Eval(IInterpreter i);
		IExpression Fold();
		bool IsReifyTimeConst();
	}
}