using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
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


		public readonly Operator Operator;


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public BinaryExpression(IExpression left, IExpression right, BinOp operation,
			LsnType type, Operator op)
		{
			Left = left; Right = right; Operation = operation; Type = type; Operator = op;
		}

		public override bool IsReifyTimeConst()
			=> Left.IsReifyTimeConst() && Right.IsReifyTimeConst();
		

		public override IExpression Fold()
		{
			Right = Right.Fold();
			Left = Left.Fold();
			//typeof(ILSN_Value).IsAssignableFrom(Left.GetType()) && typeof(ILSN_Value).IsAssignableFrom(Right.GetType())
			if (Left is ILsnValue && Right is ILsnValue)
				return Operation(Left as ILsnValue, Right as ILsnValue);
			return this;
		}

		public override ILsnValue Eval(IInterpreter i)
			=> Operation(Left.Eval(i), Right.Eval(i));

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Left.Equals(oldExpr)) _Left = newExpr;
			if (Right.Equals(oldExpr)) _Right = newExpr;
		}

		public override bool Equals(IExpression other)
		{
			var e = other as BinaryExpression;
			if (e == null) return false;
			return Operation == e.Operation && e.Left.Equals(Left) && e.Right == Right;
		}
	}
}
