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
		/// Move on to the next iteration of the (innermost) loop, similar to the C# 'continue' keyword.
		/// </summary>
		Next,
		/// <summary>
		/// Exit the innermost loop; same as the C# 'break' keyword
		/// </summary>
		Break,
		/// <summary>
		/// Return from the current function. If a value is to be returned, it will have already been assigned to the IInterpreter's
		/// return value property.
		/// </summary>
		Return
	}

	/// <summary>
	/// The basis LSN class, children include control structures and statements.
	/// </summary>
	[Serializable]
	public abstract class Component : IExpressionContainer
	{
		/// <summary>
		/// The return value is false when the script, function or loop should stop (e.g. break or return.).
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public abstract InterpretValue Interpret(IInterpreter i);


		public abstract void Replace(IExpression oldExpr, IExpression newExpr);
	}
}
