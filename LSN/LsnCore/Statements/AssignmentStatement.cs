using System;
using LsnCore.Expressions;

namespace LsnCore.Statements
{
	[Serializable]
	public class AssignmentStatement : Statement
	{
		public IExpression Value;
		//public string VariableName;
		public int Index;
		//public LSN_Type Type;
		//public bool Mutable;

		
		public AssignmentStatement(/*string name*/int index, IExpression value)
		{
			//VariableName = name;
			Index = index;
			Value = value;
		}

		
		public override InterpretValue Interpret(IInterpreter i)
		{
			//i.AddVariable(VariableName,Value.Eval(i));
			i.SetVariable(Index, Value.Eval(i));
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr)) Value = newExpr;
		}
	}
}
