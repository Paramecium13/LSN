using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Values;
using LsnCore.Types;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public sealed class FieldAssignmentStatement : Statement
	{
#if LSNR
		/// <summary>
		/// The expression for the value with fields.
		/// </summary>
		public IExpression FieldedValue { get; private set; }

		/// <summary>
		/// The index of the field.
		/// </summary>
		public readonly int Index;

		/// <summary>
		/// The value to assign to the field.
		/// </summary>
		public IExpression ValueToAssign { get; private set; }
#else
		/// <summary>
		/// The expression for the value with fields.
		/// </summary>
		private IExpression FieldedValue;

		/// <summary>
		/// The index of the field.
		/// </summary>
		private readonly int Index;

		/// <summary>
		/// The value to assign to the field.
		/// </summary>
		private IExpression ValueToAssign;

#endif

		public FieldAssignmentStatement(IExpression fieldedValue,int index,IExpression value)
		{
			FieldedValue = fieldedValue; Index = index; ValueToAssign = value;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			(FieldedValue.Eval(i).Value as IHasMutableFieldsValue).SetFieldValue(Index, ValueToAssign.Eval(i));
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (FieldedValue.Equals(oldExpr))
				FieldedValue = newExpr;
			else if (ValueToAssign.Equals(oldExpr))
				ValueToAssign = newExpr;
			else
			{
				FieldedValue.Replace(oldExpr, newExpr);
				ValueToAssign.Replace(oldExpr, newExpr);
			}
		}

		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.AssignField);
			writer.Write((ushort)Index);
			FieldedValue.Serialize(writer, resourceSerializer);
			ValueToAssign.Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return FieldedValue;
			foreach (var expr in FieldedValue.SelectMany(e => e))
				yield return expr;
			yield return ValueToAssign;
			foreach (var expr in ValueToAssign.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			FieldedValue.GetInstructions(instructionList, context.WithContext(ExpressionContext.FieldWrite));
			ValueToAssign.GetInstructions(instructionList, context.WithContext(ExpressionContext.Store));
			instructionList.AddInstruction(new SimplePreInstruction(OpCode.StoreField, (ushort)Index));
		}
	}

	//ToDo: Make a const version, where the ValueToAssign is a(n) LsnValue?
}
