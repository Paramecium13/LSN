using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;

namespace LsnCore.Expressions
{
	public interface IExpression : IExpressionContainer, IEquatable<IExpression>
	{
		TypeId Type { get; }

		LsnValue Eval(IInterpreter i);
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