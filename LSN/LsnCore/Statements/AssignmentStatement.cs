using System;
using LsnCore.Expressions;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public class AssignmentStatement : Statement
	{
		public IExpression Value;
		public int Index;

		// ToDo: Create a ProtoAssignmentStatement class in LSNr. Make it take a variable object, in case the variable's value changes during optimization.
		public AssignmentStatement(int index, IExpression value)
		{
			//VariableName = name;
			Index = index;
			Value = value;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.SetVariable(Index, Value.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr)) Value = newExpr;
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.AssignVariable);
			writer.Write((ushort)Index);
			Value.Serialize(writer, resourceSerializer);
		}
	}
}
