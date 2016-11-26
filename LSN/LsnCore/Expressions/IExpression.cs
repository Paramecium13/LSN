using LsnCore.Types;
using System;

namespace LsnCore.Expressions
{
	public interface IExpression : IExpressionContainer, IEquatable<IExpression>
	{
		TypeId Type { get; }

		ILsnValue Eval(IInterpreter i);
		IExpression Fold();
		bool IsReifyTimeConst();

		/// <summary>
		/// The evaluation of this expression has no side effects.
		/// </summary>
		/// <returns></returns>
		bool IsPure { get; }
	}
}