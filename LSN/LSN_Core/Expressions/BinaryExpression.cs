using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	/// <summary>
	/// 
	/// </summary>
	public class BinaryExpression : Expression
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

		private BinOp Operation;


		public BinaryExpression(IExpression left, IExpression right, BinOp operation,
			LSN_Type type)
		{
			Left = left; Right = right; Operation = operation; Type = type;
		}

		public override bool IsReifyTimeConst()
			=> Left.IsReifyTimeConst() && Right.IsReifyTimeConst();
		

		public override IExpression Fold()
		{
			Right = Right.Fold();
			Left = Left.Fold();
			if(typeof(ILSN_Value).IsAssignableFrom(Left.GetType()) && typeof(ILSN_Value).IsAssignableFrom(Right.GetType()))
				return Operation((ILSN_Value)Left, (ILSN_Value)Right);
			return this;
		}

		public override ILSN_Value Eval(IInterpreter i)
			=> Operation(Left.Eval(i), Right.Eval(i));
	}
}
