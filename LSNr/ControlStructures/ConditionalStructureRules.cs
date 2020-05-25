using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.ControlStructures;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;

namespace LSNr.ControlStructures
{
	public sealed class IfLetStructureRule : ControlStructureRule
	{
		public override bool PreCheck(Token t) => t.Value == "if";

		public override bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens.TestAt(1, t => t.Value == "let");

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
			var expr = Create.Express(exprTokens, script, null);
			var exprType = expr.Type.Type as OptionType;
			if (exprType == null)
				throw new LsnrParsingException(head[4], "The value of an if let structure must be of an option type.", script.Path);

			script.CurrentScope = script.CurrentScope.CreateChild();
			AssignmentStatement assignment = null;
			if (ForInCollectionLoop.CheckCollectionVariable(expr))
			{
				script.CurrentScope.CreateMaskVariable(vName, new HiddenCastExpression(expr, exprType.Contents), exprType.Contents.Type);
			}
			else
			{
				var variable = script.CurrentScope.CreateVariable(vName, exprType.Contents.Type);
				assignment = new AssignmentStatement(variable.Index, new HiddenCastExpression(expr, exprType.Contents));
				variable.Assignment = assignment;
			}
			var p = new Parser(body, script);
			p.Parse();
			var components = new List<Component>();
			if (assignment != null) components.Insert(0, assignment);
			components.AddRange(Parser.Consolidate(p.Components));

			return new IfControl(expr, components);
		}
	}

	public sealed class IfStructureRule : ControlStructureRule
	{
		public override int Order => ControlStructureRuleOrders.ElsIf;

		public override bool PreCheck(Token t) => t.Value == "if";

		public override bool Check(ISlice<Token> tokens, IPreScript script) => true;

		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var cnd = Create.Express(head.Skip(1).ToList(), script);
			script.CurrentScope = script.CurrentScope.CreateChild();
			var p = new Parser(body, script);
			p.Parse();
			var components = Parser.Consolidate(p.Components);
			script.CurrentScope = script.CurrentScope.Pop(components);
			return new IfControl(cnd, components);
		}
	}

	public sealed class ElsIfStructureRule : ControlStructureRule
	{
		public override int Order => ControlStructureRuleOrders.ElsIf;

		public override bool PreCheck(Token t) => t.Value == "else" || t.Value == "elsif";

		public override bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens[0].Value == "elsif" || tokens.TestAt(1,t => t.Value == "if");

		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var offset = head[0].Value == "else" ? 1 : 0;
			script.CurrentScope = script.CurrentScope.CreateChild();
			var p = new Parser(body, script);
			p.Parse();
			var components = Parser.Consolidate(p.Components);
			script.CurrentScope = script.CurrentScope.Pop(components);
			return new ElsifControl(Create.Express(head.Skip(1 + offset).ToList(), script), components);
		}
	}

	public sealed class ElseStructureRule : ControlStructureRule
	{
		public override int Order => ControlStructureRuleOrders.Else;

		public override bool PreCheck(Token t) => t.Value == "else";

		public override bool Check(ISlice<Token> tokens, IPreScript script) => true;

		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			script.CurrentScope = script.CurrentScope.CreateChild();
			var p = new Parser(body, script);
			p.Parse();
			var components = Parser.Consolidate(p.Components);
			script.CurrentScope = script.CurrentScope.Pop(components); // 'head.Count == 1'
			if (head.Count <= 1) return new ElseControl(components);
			script.Valid = false;
			Console.WriteLine($"Error line {head[1].LineNumber}: Unexpected token '{head[1]}'. Expected '{{'.");
			return new ElseControl(components);
		}
	}

	public sealed class ChooseStructureRule : ControlStructureRule
	{
		public override bool PreCheck(Token t) => t.Value == "choose" || t.Value == "choice";

		public override bool Check(ISlice<Token> tokens, IPreScript script) => true;

		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var p = new Parser(body, script);
			p.Parse();
			var components = Parser.Consolidate(p.Components);
			return new ChoicesBlockControl(components);
		}
	}
}
