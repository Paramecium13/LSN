using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Values;
using LSNr;
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

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (VarValue.Equals(oldExpr)) VarValue = newExpr;
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}

	public class ForInCollectionLoop : ControlStructure
	{
		public Variable Index;
		public Variable Iterator;
		public Variable Collection;

		public readonly List<Component> Body;

		public ForInCollectionLoop(Variable index, Variable iterator, Variable collection, IEnumerable<Component> body)
		{
			Index = index; Iterator = iterator; Collection = collection; Body = body.ToList();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			foreach (var item in Body)
				item.Replace(oldExpr, newExpr);
		}
	}
	public class ForInRangeLoop : ControlStructure
	{
		internal readonly Variable			Iterator;


		public readonly List<Component> Body;

		internal IExpression Start { get; set; }

		internal IExpression End { get; set; }

		// note to flattener:
		// if not null, place this before all other statements from this component.
		internal Statement Statement { get; set; }

		internal ForInRangeLoop(Variable index, IEnumerable<Component> body)
		{
			Iterator = index; Body = body.ToList();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			foreach (var item in Body)
				item.Replace(oldExpr, newExpr);
		}
	}
}
