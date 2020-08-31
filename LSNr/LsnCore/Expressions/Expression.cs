using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Collections;
using LSNr;
using LSNr.CodeGeneration;

namespace LsnCore.Expressions
{
	public abstract class Expression : IExpression
	{
		/// <inheritdoc/>
		public /*virtual*/ TypeId Type { get; protected set; }

		/// <inheritdoc/>
		public abstract bool IsPure { get; }

		/// <inheritdoc/>
		public abstract bool IsReifyTimeConst();

		/// <inheritdoc/>
		public abstract IExpression Fold();

		/// <inheritdoc/>
		public abstract IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context);

		public abstract void GetInstructions(InstructionList instructions, InstructionGenerationContext context);

		/// <inheritdoc/>
		public virtual void Replace(IExpression oldExpr, IExpression newExpr) { }
		
		/// <inheritdoc/>
		public virtual bool Equals(IExpression other) => this == other;
		
		/// <inheritdoc/>
		public abstract void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer);

		/// <inheritdoc/>
		public abstract IEnumerator<IExpression> GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
