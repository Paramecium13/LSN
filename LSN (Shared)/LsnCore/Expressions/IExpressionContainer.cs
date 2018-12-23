using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	public interface IExpressionContainer
	{
		void Replace(IExpression oldExpr, IExpression newExpr);
	}
}
