using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	/// <summary>
	/// Used in parsing to generate LSN_Expressions
	/// </summary>
	public abstract class LSN_ExpressionTree
	{
		public abstract LSN_Type ReturnType();
	}

	public abstract class LSN_BinaryExpressionTree : LSN_ExpressionTree
	{
		public LSN_ExpressionTree LeftSide;
		public LSN_ExpressionTree RightSide;

	}


}
