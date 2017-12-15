using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Values;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public sealed class FieldAssignmentStatement : Statement
	{
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

		public FieldAssignmentStatement(IExpression fieldedValue,int index,IExpression value)
		{
			FieldedValue = fieldedValue; Index = index; ValueToAssign = value;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			(FieldedValue.Eval(i).Value as IHasMutableFieldsValue).SetFieldValue(Index, ValueToAssign.Eval(i));
			return InterpretValue.Base;
		}

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

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)StatementCode.AssignField);
			writer.Write((ushort)Index);
			FieldedValue.Serialize(writer, resourceSerializer);
			ValueToAssign.Serialize(writer, resourceSerializer);
		}
	}

	// Make a const version, where the ValueToAssign is a(n) LsnValue?
}
