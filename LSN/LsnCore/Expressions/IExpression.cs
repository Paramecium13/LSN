using System;

namespace LsnCore.Expressions
{
	public interface IExpression : IExpressionContainer, IEquatable<IExpression>
	{
		LsnType Type { get; set; }

		ILsnValue Eval(IInterpreter i);
		IExpression Fold();
		bool IsReifyTimeConst();
	}
}