using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Statements;

namespace LsnCore.ControlStructures
{
	[Serializable]
	public class IfElseControl : ControlStructure
	{
		public IExpression Condition { get; set; }
		public readonly List<Component> Body;
		public readonly List<ElsifControl> Elsifs = new List<ElsifControl>();
		public readonly List<Component> ElseBlock;

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException(); //ToDo: Implement
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}

	public class IfControl : ControlStructure
	{
		public IExpression Condition { get; private set; }
		public List<Component> Body;

		public IfControl(IExpression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}
	[Serializable]
	public class ElsifControl : ControlStructure
	{
		public IExpression Condition;
		public List<Component> Body;

		public ElsifControl(IExpression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}

	public class ElseControl : ControlStructure
	{
		public IExpression Condition;

		public List<Component> Body;

		public ElseControl(IExpression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}
}
