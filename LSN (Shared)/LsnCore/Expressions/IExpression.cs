using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;

namespace LsnCore.Expressions
{
	/// <summary>
	/// An expression. It is evaluated to return a value (unless it's a void procedure call...).
	/// </summary>
	public interface IExpression : IExpressionContainer, IEquatable<IExpression>, IEnumerable<IExpression>
	{
		/// <summary>
		/// The type this expression returns.
		/// </summary>
		TypeId Type { get; }

#if CORE
		/// <summary>
		/// Evaluate this expression.
		/// </summary>
		/// <param name="i"> The interpreter. </param>
		/// <returns>The result of evaluating this expression.</returns>
		LsnValue Eval(IInterpreter i);
#endif
		/// <summary>
		/// Folds this expression. Performs optimizations such as constant folding
		/// and returns the optimized expression.
		/// </summary>
		/// <returns></returns>
		IExpression Fold();

		/// <summary>
		/// Determines whether this expression is compile time constant.
		/// </summary>
		/// <returns></returns>
		bool IsReifyTimeConst();

		/// <summary>
		/// The evaluation of this expression has no side effects.
		/// </summary>
		/// <returns></returns>
		bool IsPure { get; }

		/// <summary>
		/// Serializes this expression with the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="resourceSerializer">The resource serializer.</param>
		void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer);
	}
}