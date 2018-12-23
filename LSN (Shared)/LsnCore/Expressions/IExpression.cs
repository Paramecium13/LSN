using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;

namespace LsnCore.Expressions
{
	public interface IExpression : IExpressionContainer, IEquatable<IExpression>, IEnumerable<IExpression>
	{
		TypeId Type { get; }

#if CORE
		LsnValue Eval(IInterpreter i);
#endif
		IExpression Fold();
		bool IsReifyTimeConst();

		/// <summary>
		/// The evaluation of this expression has no side effects.
		/// </summary>
		/// <returns></returns>
		bool IsPure { get; }

		void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer);
	}
}