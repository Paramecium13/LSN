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
		public Variable Index { get; }
		
		public IExpression Collection { get; }

		/// <summary>
		/// [Optional] The statement that stores the value being looped over in a variable.
		/// </summary>
		public Statement Statement { get; set; }

		
		public readonly ICollection<Component> Body;

		public ForInCollectionLoop(Variable index, IExpression collection, IEnumerable<Component> body)
		{
			Index = index; Collection = collection; Body = body.ToList();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			foreach (var item in Body)
				item.Replace(oldExpr, newExpr);
		}

		/// <summary>
		/// Can this expression be used as is or does it need to be stored in a variable?
		/// </summary>
		/// <param name="expr"></param>
		/// <param name="recCount">The count of recursive calls. Deeply nested fields cannot be directly used.</param>
		/// <returns></returns>
		internal static bool CheckCollectionVariable(IExpression expr, int recCount = 0)
		{
			if (recCount > 8) return false;
			switch (expr)
			{
				case FieldAccessExpression f:
					return CheckCollectionVariable(f.Value, recCount + 1);
				case VariableExpression varExp:
					return !varExp.Variable.Mutable; // If it's mutable, it could be changed inside the loop.
				case GlobalVariableAccessExpression _:
				case UniqueScriptObjectAccessExpression _:
				case LsnValue _:
					return true;
				case CollectionValueAccessExpression coll:
					return CheckCollectionVariable(coll.Index, recCount + 1)
						&& CheckCollectionVariable(coll.Collection, recCount + 1);
				default:
					return false;
			}
		}
	}

	public class ForInRangeLoop : ControlStructure
	{
		internal readonly Variable				Iterator;
		public readonly ICollection<Component>	Body;

		internal IExpression Start { get; set; }

		internal IExpression End { get; set; }

		// note to flattener:
		// if not null, place this before all other statements from this component.
		internal Statement Statement { get; set; }
		// note from flattener: It has been done.

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
