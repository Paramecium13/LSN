using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	/// <summary>
	/// A syntax object that can contain <see cref="IExpression"/>s.
	/// </summary>
	public interface IExpressionContainer
	{
		/// <summary>
		/// Replaces the old expression with the new one.
		/// </summary>
		/// <param name="oldExpr">The old expr.</param>
		/// <param name="newExpr">The new expr.</param>
		void Replace(IExpression oldExpr, IExpression newExpr);
	}
}
