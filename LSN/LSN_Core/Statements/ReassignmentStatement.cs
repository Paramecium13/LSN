﻿using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core.Statements
{
	/// <summary>
	/// Assigns a new value to a variable.
	/// </summary>
	[Serializable]
	public class ReassignmentStatement : Statement
	{
		protected string Name { get; set; }

		private IExpression _Value;
		protected IExpression Value { get { return _Value; } set { _Value = value; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"> The name of the variable that is being reassigned to.</param>
		/// <param name="value"> The value that it is being assigned.</param>
		public ReassignmentStatement(string name, IExpression value)
		{
			Name = name;
			Value = value;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.ReAssignVariable(Name, Value.Eval(i));
			return InterpretValue.Base;
		}

		public void Optimise()
		{
			Value = Value.Fold();
		}

	}
}
