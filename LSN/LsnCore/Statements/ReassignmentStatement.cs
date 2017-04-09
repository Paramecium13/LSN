using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore.Statements
{
	/// <summary>
	/// Assigns a new value to a variable. A reassignment statement is valid if and only if 
	/// the variable already exists and is declared as mutable and its type matches the 
	/// return type of the expression (which itself must be valid).
	/// </summary>
	[Serializable]
	public sealed class ReassignmentStatement : Statement
	{
		//protected string Name { get; set; }

		public int Index;
		
		private IExpression _Value;
		public IExpression Value { get { return _Value; } set { _Value = value; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"> The name of the variable that is being reassigned to.</param>
		/// <param name="value"> The value that it is being assigned.</param>
		public ReassignmentStatement(/*string name*/int index, IExpression value)
		{
			//Name = name;
			Index = index;
			Value = value;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			//i.ReAssignVariable(Name, Value.Eval(i));
			i.SetValue(Index, Value.Eval(i));
			return InterpretValue.Base;
		}

		public void Optimise()
		{
			Value = Value.Fold();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (oldExpr.Equals(Value)) Value = newExpr;
		}
	}
}
