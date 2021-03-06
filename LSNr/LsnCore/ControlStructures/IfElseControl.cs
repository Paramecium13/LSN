﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Statements;

namespace LsnCore.ControlStructures
{
	public class IfElseControl : ControlStructure
	{
		public IExpression Condition { get; set; }
		public readonly IList<Component> Body;
		public readonly IList<ElsifControl> Elsifs = new List<ElsifControl>();
		public List<Component> ElseBlock; // Can be null

		public IfElseControl(IExpression condition, IList<Component> body)
		{
			Condition = condition; Body = body;
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

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}

	public class ElsifControl : ControlStructure
	{
		public IExpression Condition;
		public List<Component> Body;

		public ElsifControl(IExpression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}

	public class ElseControl : ControlStructure
	{
		public List<Component> Body;

		public ElseControl(List<Component> body)
		{
			Body = body;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}
	}
}
