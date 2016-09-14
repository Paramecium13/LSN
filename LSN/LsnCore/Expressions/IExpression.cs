using System;

namespace LsnCore.Expressions
{
	public interface IExpression : IExpressionContainer, IEquatable<IExpression>
	{
		LsnType Type { get; }

		ILsnValue Eval(IInterpreter i);
		IExpression Fold();
		bool IsReifyTimeConst();
	}
}