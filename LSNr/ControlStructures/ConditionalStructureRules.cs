using System;
using System.Collections.Generic;
using System.Linq;
using LsnCore;
using LsnCore.ControlStructures;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;

namespace LSNr.ControlStructures
{
	/// <summary>
	/// A <see cref="ControlStructureRule"/> for 'If-Let' control structures.
	/// </summary>
	/// <remarks>
	/// Returns a <see cref="IfControl"/>, with a scope including the variable from the 'If-Let', potentially with an <see cref="AssignmentStatement"/>.
	/// </remarks>
	/// <seealso cref="IfStructureRule" />
	public sealed class IfLetStructureRule : ControlStructureRule
	{
		/// <inheritdoc/>
		public override bool PreCheck(Token t) => t.Value == "if";

		/// <inheritdoc/>
		public override bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens.TestAt(1, t => t.Value == "let");

		/// <inheritdoc/>
		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			// if  let  x  =  ...    {
			// 0   1    2  3  [4,n)  n
			if (head[2].Value == "mut" && head[3].Value != "=")
				throw new LsnrParsingException(head[2], "cannot declare the variable of an if let structure as mutable.", script.Path);
			if (head.Count < 5 || head[3].Value != "=")
				throw new LsnrParsingException(head[1], "improperly formatted 'if let' structure.", script.Path);
			var vName = head[2].Value;
			var exprTokens = head.CreateSliceAt(4);
			var expr = Create.Express(exprTokens, script);
			if (!(expr.Type.Type is OptionType exprType))
				throw new LsnrParsingException(head[4], "The value of an if let structure must be of an option type.", script.Path);

			script.PushScope();
			AssignmentStatement assignment = null;
			if (ForInCollectionLoop.CheckCollectionVariable(expr))
			{
				script.CurrentScope.CreateMaskVariable(vName, new HiddenCastExpression(expr, exprType.Contents), exprType.Contents.Type);
			}
			else
			{
				// ToDo: Move this logic into AssignmentStatement, IScope, or Variable.
				var variable = script.CurrentScope.CreateVariable(vName, exprType.Contents.Type);
				assignment = new AssignmentStatement(variable, new HiddenCastExpression(expr, exprType.Contents));
				variable.Assignment = assignment;
			}
			var p = new Parser(body, script);
			p.Parse();
			var components = new List<Component>();
			if (assignment != null) components.Add(assignment);
			components.AddRange(Parser.Consolidate(p.Components));
			script.PopScope(components);
			return new IfControl(expr, components);
		}
	}

	public sealed class IfStructureRule : ControlStructureRule
	{
		/// <inheritdoc/>
		public override int Order => ControlStructureRuleOrders.ElsIf;

		/// <inheritdoc/>
		public override bool PreCheck(Token t) => t.Value == "if";

		/// <inheritdoc/>
		public override bool Check(ISlice<Token> tokens, IPreScript script) => true;

		/// <inheritdoc/>
		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var cnd = Create.Express(head.Skip(1).ToList(), script);
			script.PushScope();
			var components = Parser.Parse(body, script);
			script.PopScope(components);
			return new IfControl(cnd, components);
		}
	}

	public sealed class ElsIfStructureRule : ControlStructureRule
	{
		/// <inheritdoc/>
		public override int Order => ControlStructureRuleOrders.ElsIf;

		/// <inheritdoc/>
		public override bool PreCheck(Token t) => t.Value == "else" || t.Value == "elsif";

		/// <inheritdoc/>
		public override bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens[0].Value == "elsif" || tokens.TestAt(1,t => t.Value == "if");

		/// <inheritdoc/>
		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var offset = head[0].Value == "else" ? 1 : 0;
			script.PushScope();
			var components = Parser.Parse(body,script);
			script.PopScope(components);
			return new ElsifControl(Create.Express(head.Skip(1 + offset).ToList(), script), components);
		}
	}

	public sealed class ElseStructureRule : ControlStructureRule
	{
		/// <inheritdoc/>
		public override int Order => ControlStructureRuleOrders.Else;

		/// <inheritdoc/>
		public override bool PreCheck(Token t) => t.Value == "else";

		/// <inheritdoc/>
		public override bool Check(ISlice<Token> tokens, IPreScript script) => true;

		/// <inheritdoc/>
		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			script.PushScope();
			var components = Parser.Parse(body, script);
			script.PopScope(components); // 'head.Count == 1'
			if (head.Count <= 1) return new ElseControl(components);
			script.Valid = false;
			Console.WriteLine($"Error line {head[1].LineNumber}: Unexpected token '{head[1]}'. Expected '{{'.");
			return new ElseControl(components);
		}
	}

	public sealed class ChooseStructureRule : ControlStructureRule
	{
		/// <inheritdoc/>
		public override bool PreCheck(Token t) => t.Value == "choose" || t.Value == "choice";

		/// <inheritdoc/>
		public override bool Check(ISlice<Token> tokens, IPreScript script) => true;

		/// <inheritdoc/>
		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var p = new Parser(body, script);
			p.Parse();
			var components = Parser.Consolidate(p.Components);
			return new ChoicesBlockControl(components);
		}
	}
}
