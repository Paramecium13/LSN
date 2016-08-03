using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public class OrExpression : Expression
	{
		private IExpression _Left;

		/// <summary>
		/// The left hand side of this expression.
		/// </summary>
		public IExpression Left { get { return _Left; } set { _Left = value; } }

		private IExpression _Right;

		/// <summary>
		/// The right hand side of this expression.
		/// </summary>
		public IExpression Right { get { return _Right; } set { _Right = value; } }


		public OrExpression(IExpression left,IExpression right, LsnType type)
		{

		}

		public override bool IsReifyTimeConst()
			=> Left.IsReifyTimeConst() && Right.IsReifyTimeConst();

		public override IExpression Fold()
		{
			Right = Right.Fold();
			Left = Left.Fold();
			if (Left is ILsnValue) // Is the left side constant? typeof(ILSN_Value).IsAssignableFrom(Left.GetType())
			{
				if ((Left as ILsnValue).BoolValue) return Left;
				return Right; // "false || Foo(x)" => "Foo(x)"
			}
			return this;
        }

		public override ILsnValue Eval(IInterpreter i)
		{
			var l = Left.Eval(i);
			if (l.BoolValue) return l;
			var r = Right.Eval(i);
			if (r.BoolValue) return r;
			return LSN_BoolValue.GetBoolValue(false);
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Left.Equals(oldExpr)) _Left = newExpr;
			if (Right.Equals(oldExpr)) _Right = newExpr;
		}

		public override bool Equals(IExpression other)
		{
			var e = other as OrExpression;
			if (e == null) return false;
			return e.Left.Equals(Left) && e.Right == Right;
		}
	}
}
