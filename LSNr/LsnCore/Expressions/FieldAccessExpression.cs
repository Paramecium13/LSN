using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class FieldAccessExpression : Expression
	{
		public IExpression Value;
		internal readonly int Index;

		public override bool IsPure => true;

		/// <inheritdoc />
		public FieldAccessExpression(IExpression value, string name, LsnType type)
		{
			Value = value;
			Index = ((IHasFieldsType)value.Type.Type).GetIndex(name);
			Type = type.Id;
		}

		/// <inheritdoc />
		public FieldAccessExpression(IExpression value, string name, TypeId type)
		{
			Value = value;
			Index = ((IHasFieldsType)value.Type.Type).GetIndex(name);
			Type = type;
		}

		/// <inheritdoc />
		internal FieldAccessExpression(IExpression fieldOwner, Field field)
		{
			Value = fieldOwner; Index = field.Index; Type = field.Type;
		}

		/// <inheritdoc />
		public FieldAccessExpression(IExpression value, int index)
		{
			Value = value;
			Index = index;
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst()
			=> false;

		/// <inheritdoc />
		public override IExpression Fold()
		{
			Value = Value.Fold();
			return this;
		}

		/// <inheritdoc />
		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			var subContext = context.WithContext(ExpressionContext.SubExpression);
			Value.GetInstructions(instructions, subContext);
			instructions.AddInstruction(new SimplePreInstruction(OpCode.LoadField, (ushort) Index));
		}

		/// <inheritdoc />
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr)) Value = newExpr;
		}

		/// <inheritdoc />
		public override bool Equals(IExpression other)
		{
			if (!(other is FieldAccessExpression e)) return false;
			return Index == e.Index && Value.Equals(e.Value);
		}

		/// <inheritdoc />
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.FieldAccess);
			writer.Write((ushort)Index);
			Value.Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}
	}
}
