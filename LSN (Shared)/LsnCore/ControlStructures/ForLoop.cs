using LsnCore.Expressions;
using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.ControlStructures
{
	/// <summary>
	/// A(n) LSN for loop...
	/// </summary>
	public class ForLoop : ControlStructure
	{
		public int Index;

		public IExpression VarValue;

		public IExpression Condition;

		public readonly List<Component> Body;

		public Statement Post;

		public ForLoop(int index, IExpression val, IExpression con, List<Component> body, Statement post)
		{
			Index = index; VarValue = val; Condition = con; Body = body; Post = post;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new InvalidOperationException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (VarValue.Equals(oldExpr)) VarValue = newExpr;
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}
}
