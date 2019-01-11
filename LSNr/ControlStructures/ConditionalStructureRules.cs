using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.ControlStructures;
using LsnCore.Utilities;

namespace LSNr.ControlStructures
{
	public sealed class IfStructureRule : ControlStructureRule
	{
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
			if (head.Count > 1)
			{
				script.Valid = false;
				Console.WriteLine($"Error line {head[1].LineNumber}: Unexpected token '{head[1]}'. Expected '{{'.");
			}
			return new ElseControl(components);
		}
	}

	public sealed class ChooseeStructureRule : ControlStructureRule
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
