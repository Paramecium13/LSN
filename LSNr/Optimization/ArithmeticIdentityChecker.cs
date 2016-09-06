using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LSNr.Optimization
{
	public class ArithmeticIdentityChecker : ExpressionWalker
	{
		protected override IExpression View(BinaryExpression b)
		{
			if(b.Left.IsReifyTimeConst() || b.Right.IsReifyTimeConst())
				switch (b.Operator)
				{
					case LsnCore.Operator.Add:
						return CheckAdditive(b);
					case LsnCore.Operator.Subtract:
						return CheckAdditive(b);
					case LsnCore.Operator.Multiply:
						return CheckMult(b);
					case LsnCore.Operator.Divide:
						return CheckDiv(b);
					case LsnCore.Operator.Mod:
						return CheckMod(b);
					case LsnCore.Operator.Power:
						return CheckPow(b);
					default:
						break;
				}
			return b;
		}


		private IExpression CheckAdditive(BinaryExpression b)
		{
			var lintq = b.Left as IntValue?;
			var ldoubleq = b.Left as DoubleValue?;
			var lstrq = b.Left as StringValue?;
			var rintq = b.Right as IntValue?;
			var rdoubleq = b.Right as DoubleValue?;
			var rstrq = b.Right as StringValue?;
			if (lintq != null)
			{
				var lint = (IntValue)lintq;
				if (lint.Value == 0)
					return b.Right;
			}
			if (rintq != null && ((IntValue)rintq).Value == 0)
					return b.Left;
			if (ldoubleq != null)
			{
				var ldouble = (DoubleValue)ldoubleq;
				if (ldouble.Value == 0)
					return b.Right;
			}
			if (rdoubleq != null && ((DoubleValue)rdoubleq).Value == 0)
					return b.Left;
			if (lstrq != null)
			{
				var lstr = (StringValue)lstrq;
				if (lstr.Value == "")
					return b.Right;
				if (rstrq != null && ((StringValue)rstrq).Value == "")
					return lstr;
				return b;
			}
			return b;
		}


		private IExpression CheckMult(BinaryExpression b)
		{
			var lintq = b.Left as IntValue?;
			var ldoubleq = b.Left as DoubleValue?;
			var rintq = b.Right as IntValue?;
			var rdoubleq = b.Right as DoubleValue?;
			if (lintq != null)
			{
				var lint = (IntValue)lintq;
				if (lint.Value == 1)
					return b.Right;
				if (lint.Value == 0)
					return new IntValue(0);
				
			}
			if (rintq != null)
			{
				int i = ((IntValue)rintq).Value;
				if (i == 1) return b.Left;
				if(i ==0) return new IntValue(0);
			}
			if (ldoubleq != null)
			{
				var ldouble = (DoubleValue)ldoubleq;
				if (ldouble.Value == 1)
					return b.Right;
				if (ldouble.Value == 0)
					return new DoubleValue(0);
			}
			if (rdoubleq != null)
			{
				double d = ((DoubleValue)rdoubleq).Value;
				if (d == 0)
						return new DoubleValue(0);
				if (d == 1)
					return b.Left;
			}
			return b;
		}


		private IExpression CheckDiv(BinaryExpression b)
		{
			return b;
		}


		private IExpression CheckMod(BinaryExpression b)
		{
			return b;
		}


		private IExpression CheckPow(BinaryExpression b)
		{
			var lintq = b.Left as IntValue?;
			var ldoubleq = b.Left as DoubleValue?;
			var rintq = b.Right as IntValue?;
			var rdoubleq = b.Right as DoubleValue?;
			if (lintq != null)
			{
				var lint = (IntValue)lintq;
				if (lint.Value == 1)
					return new IntValue(1);
				if (lint.Value == 0)
					return new IntValue(0);

			}
			if (rintq != null)
			{
				int i = ((IntValue)rintq).Value;
				if (i == 1) return b.Left;
				if (i == 0) return new IntValue(1);
			}
			if (ldoubleq != null)
			{
				var ldouble = (DoubleValue)ldoubleq;
				if (ldouble.Value == 1)
					return new DoubleValue(1);
				if (ldouble.Value == 0)
					return new DoubleValue(0);
			}
			if (rdoubleq != null)
			{
				double d = ((DoubleValue)rdoubleq).Value;
				if (d == 0)
					return new DoubleValue(1);
				if (d == 1)
					return b.Left;
			}
			return b;
		}


	}
}
