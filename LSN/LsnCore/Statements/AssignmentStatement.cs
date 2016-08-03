using System;
using LsnCore.Expressions;

namespace LsnCore.Statements
{
	[Serializable]
	public class AssignmentStatement : Statement
	{
		private IExpression Value;
		public string VariableName;
		//public LSN_Type Type;
		//public bool Mutable;

		public AssignmentStatement(string name, IExpression value)
		{
			VariableName = name;
			Value = value;
		}

		
		public override InterpretValue Interpret(IInterpreter i)
		{
			i.AddVariable(VariableName,Value.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr)) Value = newExpr;
		}
	}
}
