﻿using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr;
using Syroot.BinaryData;
using LSNr.CodeGeneration;

namespace LsnCore.Expressions
{
	/// <summary>
	/// Access a value in a collection.
	/// </summary>
	public sealed class CollectionValueAccessExpression : Expression
	{
		public IExpression Collection;
		public IExpression Index;

		/// <inheritdoc/>
		public override bool IsPure => true;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collection"> The collection expression.</param>
		/// <param name="index"> The index expression.</param>
		/// <param name="type"> The type of the value contained in the collection.</param>
		public CollectionValueAccessExpression(IExpression collection, IExpression index, TypeId type)
		{
			Collection = collection; Index = index; Type = type;
		}

		/// <inheritdoc/>
		public override bool Equals(IExpression other)
		{
			if (!(other is CollectionValueAccessExpression e)) return false;
			return e.Collection == Collection && e.Index == Index;
		}

		/// <inheritdoc />
		public override IExpression Fold()
		{
			var c = Collection.Fold();
			var i = Index.Fold();
			if (i == Index && c == Collection) return this;
			IExpression expr;       // typeof(ICollectionValue).IsAssignableFrom(c.GetType())
			if (i.IsReifyTimeConst() && c is LsnValue val && val.Value is ICollectionValue cl)
			{
				try
				{
					expr = cl.GetValue(((LsnValue)i).IntValue);
				}
				catch
				{
					expr = new CollectionValueAccessExpression(c, i, Type);
				}
			}
			else expr = new CollectionValueAccessExpression(c, i, Type);
			return expr;
		}

		/// <inheritdoc />
		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			var subContext = context.WithContext(ExpressionContext.SubExpression);
			foreach (var instruction in Collection.GetInstructions(subContext))
			{
				yield return instruction;
			}

			foreach (var instruction in Index.GetInstructions(subContext))
			{
				yield return instruction;
			}

			yield return new SimplePreInstruction(OpCode.LoadElement, 0);
			if (Type.Type is StructType)
			{
				switch (context.Context)
				{
					case ExpressionContext.Store:
					case ExpressionContext.Parameter_Default:
						yield return new SimplePreInstruction(OpCode.CopyStruct, 0);
						break;
				}
			}
			
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			var subContext = context.WithContext(ExpressionContext.SubExpression);
			Collection.GetInstructions(instructions, subContext);
			Index.GetInstructions(instructions, subContext);
			instructions.AddInstruction(new SimplePreInstruction(OpCode.LoadElement, 0));
			if (Type.Type is StructType)
			{
				switch (context.Context)
				{
					case ExpressionContext.Store:
					case ExpressionContext.Parameter_Default:
						instructions.AddInstruction(new SimplePreInstruction(OpCode.CopyStruct, 0));
						break;
				}
			}
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst()
			=> Collection.IsReifyTimeConst() && Index.IsReifyTimeConst();

		/// <inheritdoc/>
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Collection.Equals(oldExpr)) Collection = newExpr;
			if (Index.Equals( oldExpr)) Index = newExpr;
		}

		/// <inheritdoc/>
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.CollectionValueAccess);
			Collection.Serialize(writer, resourceSerializer);
			Index.Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc/>
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Collection;
			foreach (var expr in Collection.SelectMany(e => e))
				yield return expr;
			yield return Index;
			foreach (var expr in Index.SelectMany(e => e))
				yield return expr;
		}
	}
}