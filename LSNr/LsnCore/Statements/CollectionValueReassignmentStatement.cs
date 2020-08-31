using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using LsnCore.Expressions;
using LSNr;
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
		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
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
		protected override IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context)
		{
			foreach (var instruction in Collection.GetInstructions(context.WithContext(ExpressionContext.ItemWrite)))
			{
				yield return instruction;
			}

			foreach (var instruction in Index.GetInstructions(context.WithContext(ExpressionContext.SubExpression)))
			{
				yield return instruction;
			}

			foreach (var instruction in Value.GetInstructions(context.WithContext(ExpressionContext.Store)))
			{
				yield return instruction;
			}

			yield return new SimplePreInstruction(OpCode.StoreElement, 0);
		}
	}

	// Make const versions, where Index and/or Value are constant.
}
