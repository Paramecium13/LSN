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
	//TODO: Check use this on expressions before they are placed in a component and then fold them.
	//TODO: Make this check boolean expressions...
		//e.g. Check for simple tautology (x || !x).
	
	public class ArithmeticIdentityChecker : ExpressionWalker
	{
		protected override IExpression WalkBinExp(BinaryExpression bin)
		{
			var x = base.WalkBinExp(bin);
			var b = x as BinaryExpression;
			if(b != null)
			{
				if (b.Left.IsReifyTimeConst() || b.Right.IsReifyTimeConst())
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
			return x;
		}


		private static IExpression CheckAdditive(BinaryExpression b)
		{
			var leftType = b.Left.Type;
			var rightType = b.Right.Type;

			if(b.Left is LsnValue?)
			{
				if (leftType == LsnType.int_.Id)
				{
					if (((LsnValue)b.Left).IntValue == 0) return b.Right;
				}
				else if (leftType == LsnType.double_.Id)
				{
					if (((LsnValue)b.Left).DoubleValue == 0) return b.Right;
				}
				else if (leftType == LsnType.string_.Id)
				{
					if (((StringValue)((LsnValue)b.Left).Value).Value == "") return b.Right;
				}
			}
			else if(b.Right is LsnValue?)
			{
				if (rightType == LsnType.int_.Id)
				{
					if (((LsnValue)b.Right).IntValue == 0) return b.Left;
				}
				else if (rightType == LsnType.double_.Id)
				{
					if (((LsnValue)b.Right).DoubleValue == 0) return b.Left;
				}
				else if (rightType == LsnType.string_.Id)
				{
					if (((StringValue)((LsnValue)b.Right).Value).Value == "") return b.Left;
				}
			}

			return b;
		}

		private static IExpression CheckDiff(BinaryExpression b)
		{
			var leftType = b.Left.Type;
			var rightType = b.Right.Type;

			if (b.Left is LsnValue?)
			{
				if (leftType == LsnType.int_.Id)
				{
					if (((LsnValue)b.Left).IntValue == 0) return b.Right;
				}
				else if (leftType == LsnType.double_.Id)
				{
					if (((LsnValue)b.Left).DoubleValue == 0) return b.Right;
				}
			}
			else if (b.Right is LsnValue?)
			{
				if (rightType == LsnType.int_.Id)
				{
					if (((LsnValue)b.Right).IntValue == 0) return b.Left;
				}
				else if (rightType == LsnType.double_.Id)
				{
					if (((LsnValue)b.Right).DoubleValue == 0) return b.Left;
				}
			}

			return b;
		}

		private static IExpression CheckMult(BinaryExpression b)
		{
			var leftType = b.Left.Type;
			var rightType = b.Right.Type;

			if (b.Left is LsnValue?)
			{
				if (leftType == LsnType.int_.Id)
				{

				}
				else if (leftType == LsnType.double_.Id)
				{

				}
				else if (leftType == LsnType.string_.Id)
				{

				}
			}
			else if (b.Right is LsnValue?)
			{
				if (rightType == LsnType.int_.Id)
				{

				}
				
			}
			return b;
		}


		private static IExpression CheckDiv(BinaryExpression b)
		{
			var leftType = b.Left.Type;
			var rightType = b.Right.Type;

			if (b.Left is LsnValue?)
			{
				if (leftType == LsnType.int_.Id)
				{
					if (((LsnValue)b.Left).IntValue == 1) return b.Right;
					if (((LsnValue)b.Left).IntValue == 0) return b.Type.Type.CreateDefaultValue();
				}
				else if (leftType == LsnType.double_.Id)
				{
					if (((LsnValue)b.Left).DoubleValue == 1) return b.Right;
					if (((LsnValue)b.Left).DoubleValue == 0) return b.Type.Type.CreateDefaultValue();
				}
				else if (leftType == LsnType.string_.Id)
				{
					if (((StringValue)((LsnValue)b.Left).Value).Value == "") return b.Type.Type.CreateDefaultValue();
				}
			}
			else if (b.Right is LsnValue?)
			{
				if (rightType == LsnType.int_.Id)
				{
					if (((LsnValue)b.Right).IntValue == 1) return b.Left;
					if (((LsnValue)b.Right).IntValue == 0) return b.Type.Type.CreateDefaultValue();
				}
				else if (rightType == LsnType.double_.Id)
				{
					if (((LsnValue)b.Right).DoubleValue == 1) return b.Left;
					if (((LsnValue)b.Right).DoubleValue == 0) return b.Type.Type.CreateDefaultValue();
				}
				else if (rightType == LsnType.string_.Id)
				{
					if (((StringValue)((LsnValue)b.Right).Value).Value == "") return b.Type.Type.CreateDefaultValue();
				}
			}
			return b;
		}


		private static IExpression CheckMod(BinaryExpression b)
		{
			var leftType = b.Left.Type;
			var rightType = b.Right.Type;

			if (b.Left is LsnValue?)
			{
				if (leftType == LsnType.int_.Id)
				{

				}
				else if (leftType == LsnType.double_.Id)
				{

				}
			}
			else if (b.Right is LsnValue?)
			{
				if (rightType == LsnType.int_.Id)
				{

				}
				else if (rightType == LsnType.double_.Id)
				{

				}
			}
			return b;
		}


		private static IExpression CheckPow(BinaryExpression b)
		{
			var leftType = b.Left.Type;
			var rightType = b.Right.Type;

			if (b.Left is LsnValue?)
			{
				if (leftType == LsnType.int_.Id)
				{

				}
				else if (leftType == LsnType.double_.Id)
				{

				}
			}
			else if (b.Right is LsnValue?)
			{
				if (rightType == LsnType.int_.Id)
				{

				}
				else if (rightType == LsnType.double_.Id)
				{

				}
			}
			return b;
		}


	}
}
