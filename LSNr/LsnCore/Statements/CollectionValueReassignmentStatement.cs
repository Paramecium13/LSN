using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using LsnCore.Expressions;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public sealed class CollectionValueAssignmentStatement : Statement
	{
		private IExpression Collection;
		private IExpression Index;
		private IExpression Value;

		/// <inheritdoc />
		public CollectionValueAssignmentStatement(IExpression collection, IExpression index, IExpression value)
		{
			Collection = collection; Index = index; Value = value;
		}

		/// <inheritdoc />
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Collection.Equals(oldExpr))
				Collection = newExpr;
			else if (Index.Equals(oldExpr))
				Index = newExpr;
			else if (Value.Equals(oldExpr))
				Value = newExpr;
			else
			{
				Collection.Replace(oldExpr, newExpr);
				Index.Replace(oldExpr, newExpr);
				Value.Replace(oldExpr, newExpr);
			}
		}

		/// <inheritdoc />
		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.AssignValueInCollection);
			Collection.Serialize(writer, resourceSerializer);
			Index.Serialize(writer, resourceSerializer);
			Value.Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Collection;
			foreach (var expr in Collection.SelectMany(e => e))
				yield return expr;

			yield return Index;
			foreach (var expr in Index.SelectMany(e => e))
				yield return expr;

			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			Collection.GetInstructions(instructionList, context.WithContext(ExpressionContext.ItemWrite));
			Index.GetInstructions(instructionList, context.WithContext(ExpressionContext.SubExpression));
			Value.GetInstructions(instructionList, context.WithContext(ExpressionContext.Store));
		}
	}

	// Make const versions, where Index and/or Value are constant.
}
