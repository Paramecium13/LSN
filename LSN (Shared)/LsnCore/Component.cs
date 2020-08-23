using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	/// <summary>
	/// The value returned by Component.Interpret(IInterpreter).
	/// </summary>
	public enum InterpretValue
	{
		/// <summary>
		/// Continue as normal.
		/// </summary>
		Base,
		/// <summary>
		/// Return from the current function. If a value is to be returned, it will have already been assigned to the IInterpreter's
		/// return value property.
		/// </summary>
		Return
	}

	/// <summary>
	/// A base LSN class; its children are control structures and statements.
	/// </summary>
	public abstract class Component : IExpressionContainer
	{
#if CORE
		/// <summary>
		/// The return value is false when the script, function or loop should stop (e.g. break or return.).
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public abstract InterpretValue Interpret(IInterpreter i);
#endif

		/// <inheritdoc/>
		public abstract void Replace(IExpression oldExpr, IExpression newExpr);
	}
}
