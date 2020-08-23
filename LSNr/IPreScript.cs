using LsnCore;
using LsnCore.Types;
using LSNr.ControlStructures;
using LSNr.Statements;
using System.Collections.Generic;
using LsnCore.ControlStructures;
using LsnCore.Utilities;

namespace LSNr
{
	// All variable,field,etc. type symbols in this enum must come before Function.	
	/// <summary>
	/// The type of symbol a <see cref="TokenType.Identifier"/> <see cref="Token"/> represents.
	/// </summary>
	public enum SymbolType { Undefined, UniqueScriptObject, Variable, GlobalVariable, Field, Property, ScriptClassMethod, HostInterfaceMethod, Function, Type}

	/// <summary>
	/// A pre-script. Base interface for stuff...
	/// </summary>
	/// <seealso cref="ITypeContainer" />
	public interface IPreScript : ITypeContainer
	{
		/// <summary>
		/// Gets or sets the current scope.
		/// </summary>
		IScope CurrentScope { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="IPreScript"/> is valid.
		/// </summary>
		bool Valid { get; set; }

		/// <summary>
		/// Gets the path of the source file.
		/// </summary>
		string Path { get; }

		/// <summary>
		/// Checks the symbol.
		/// </summary>
		/// <param name="name">The name of the symbol.</param>
		/// <returns></returns>
		SymbolType CheckSymbol(string name);

		/// <summary>
		/// Gets the function with the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		Function GetFunction(string name);

		/// <summary>
		/// Gets the statement rules.
		/// </summary>
		IReadOnlyList<IStatementRule> StatementRules { get; }

		/// <summary>
		/// Gets the control structure rules.
		/// </summary>
		IReadOnlyList<ControlStructureRule> ControlStructureRules { get; }
	}

	public static class PreScriptExtensions
	{
		/// <summary>
		/// Creates a new child scope of the current scope.
		/// </summary>
		/// <param name="script">The script.</param>
		public static void PushScope(this IPreScript script)
		{
			script.CurrentScope = script.CurrentScope.CreateChild();
		}

		/// <summary>
		/// Pops the scope.
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="components">The components.</param>
		public static void PopScope(this IPreScript script, List<Component> components)
		{
			script.CurrentScope = script.CurrentScope.Pop(components);
		}

		/// <summary>
		/// Creates a control structure.
		/// </summary>
		/// <param name="script"> The script.</param>
		/// <param name="head"> The head tokens.</param>
		/// <param name="body"> The body tokens.</param>
		/// <returns></returns>
		public static ControlStructure ControlStructure(this IPreScript script, ISlice<Token> head, ISlice<Token> body)
		{
			var first = head[0];
			foreach (var rule in script.ControlStructureRules)
			{
				if (rule.PreCheck(first) && rule.Check(head, script))
					return rule.Apply(head, body, script);
			}
			throw new LsnrParsingException(first, "Not a valid control structure.", script.Path);
		}
	}
}