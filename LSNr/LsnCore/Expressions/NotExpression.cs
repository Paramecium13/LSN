using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LsnCore.Types;
using LSNr;
using MoreLinq;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class NotExpression : IExpression
	{
		internal IExpression Value;

		/// <inheritdoc />
		public TypeId Type => LsnType.Bool_.Id;

		/// <inheritdoc />
		public bool IsPure => Value.IsPure;

		internal NotExpression(IExpression value)
		{
			Value = value;
		}

		/// <inheritdoc />
		public bool Equals(IExpression other)
		{
			return other is NotExpression n && Value.Equals(n.Value);
		}

#if CORE
		public LsnValue Eval(IInterpreter i)
		{
			return LsnBoolValue.GetBoolValue(!Value.Eval(i).BoolValue);
		}
#endif

		/// <inheritdoc />
		public IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			return Value.GetInstructions(context).Append(new SimplePreInstruction(OpCode.Not, 0));
		}

		/// <inheritdoc />
		public IExpression Fold()
		{
			Value = Value.Fold();
			switch (Value)
			{
				case LsnValue c:
					return LsnBoolValue.GetBoolValue(!c.BoolValue);
				case NotExpression n:
					return n.Value;
				default:
					return this;
			}
		}

		/// <inheritdoc />
		public bool IsReifyTimeConst()
			=> Value.IsReifyTimeConst();

		/// <inheritdoc />
		public void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr))
				Value = newExpr;
			else
				Value.Replace(oldExpr, newExpr);
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.NotExpression);
			Value.Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc />
		public IEnumerator<IExpression> GetEnumerator()
		{
			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
