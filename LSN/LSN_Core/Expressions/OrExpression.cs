﻿using LSN_Core.Types;
using LSN_Core.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
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


		public OrExpression(IExpression left,IExpression right, LSN_Type type)
		{

		}

		public override bool IsReifyTimeConst()
			=> Left.IsReifyTimeConst() && Right.IsReifyTimeConst();

		public override IExpression Fold()
		{
			Right = Right.Fold();
			Left = Left.Fold();
			if (typeof(ILSN_Value).IsAssignableFrom(Left.GetType())) // Is the left side constant?
			{
				if (((ILSN_Value)Left).BoolValue) return Left;
				return Right; // "false || Foo(x)" => "Foo(x)"
			}
			return this;
        }

		public override ILSN_Value Eval(IInterpreter i)
		{
			var l = Left.Eval(i);
			if (l.BoolValue) return l;
			var r = Right.Eval(i);
			if (r.BoolValue) return r;
			return LSN_BoolValue.GetBoolValue(false);
		}
	}
}
