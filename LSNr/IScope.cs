using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using System.Collections.Generic;

namespace LSNr
{
	/// <summary>
	/// Used to create and store <see cref="Variable"/>s.
	/// </summary>
	public interface IScope
	{
		/// <summary>
		/// Creates a variable using the initial value expresion to determine the type.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="mutable">Is the variable mutable?</param>
		/// <param name="init">The expression that generates the varaiable's initial value.</param>
		Variable CreateVariable(string name, bool mutable, IExpression init);

		/// <summary>
		/// Creates a the variable with the given <paramref name="name"/> and <paramref name="type"/>.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="type">The type.</param>
		/// <param name="mutable">Is the variable mutable?</param>
		Variable CreateVariable(string name, LsnType type, bool mutable = false);

		/// <summary>
		/// Creates a variable for a procedure's parameter.
		/// </summary>
		/// <param name="param">The parameter.</param>
		Variable CreateVariable(Parameter param);

		/// <summary>
		/// Creates an iterator pseudo-variable, that accesses <paramref name="collection"/> at <paramref name="index"/>.
		/// Used in 'For In' loops.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="collection">The collection expression. Should be a field or variable.</param>
		/// <param name="index">The index variable.</param>
		/// <returns></returns>
		Variable CreateIteratorVariable(string name, IExpression collection, Variable index);

		// if let x = <<foo>>
		// type of x = y
		// type of foo = y?
		// x is a mask variable around <<foo>>. If foo is to complicated (e.g. deeply nested field, function return value), x actually stores
		// a value, otherwise it is just a hidden cast of <<foo>> to type 'y'.		
		/// <summary>
		/// Creates the a variable for use in 'if-let' structures..
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="accessExpression">The expression used to access its value. It must be a 'simple' expression.</param>
		/// <param name="type">The type of the variable.</param>
		/// <returns></returns>
		Variable CreateMaskVariable(string name, IExpression accessExpression, LsnType type);

		/// <summary>
		/// Gets the variable named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		Variable GetVariable(string name);

		/// <summary>
		/// Determines whether this scope directly has a variable named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name.</param>
		bool HasVariable(string name);

		/// <summary>
		/// Determines whether this scope or any of its parent scopes have a variable named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		bool VariableExists(string name);

		/// <summary>
		/// Called when this scope closes. Optimizes the contained variables...
		/// </summary>
		/// <param name="components">The components.</param>
		/// <returns> The parent scope. </returns>
		IScope Pop(List<Component> components);

		/// <summary>
		/// Creates a child scope.
		/// </summary>
		/// <returns></returns>
		IScope CreateChild();
	}
}