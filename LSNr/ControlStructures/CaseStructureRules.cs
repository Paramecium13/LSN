using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.ControlStructures;
using LsnCore.Utilities;
using LSNr.LssParser;

namespace LSNr.ControlStructures
{
	public sealed class ConditionedChoiceStructureRule : ControlStructureRule
	{
		public override int Order => ControlStructureRuleOrders.Base;

		public override bool PreCheck(Token t) => t.Value == "when";

		public override bool Check(ISlice<Token> tokens, IPreScript script) => true;

		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			/* when [expression (condition)] : [expression] -> [block] */
			// It's a conditional choice (inside a choice block).
			var (tokens, substitutions) = ExpressionParser.MultiParse(head.ToArray(), script);
			// {"when", condition, :, title, ->}
			// validate
			if (tokens.Length != 5 || tokens[1].Type != TokenType.Substitution || tokens[3].Type != TokenType.Substitution)
				throw new LsnrParsingException(head[0], "Improperly formatted conditional choice.\r\n Proper format: when 'cond' : 'label' -> {...", script.Path);
			if (tokens[4].Value != "->")
				//throw new LsnrParsingException(head[0], "Improperly formatted conditional choice: must end in '->'.", script.Path);
				throw LsnrParsingException.UnexpectedToken(tokens[4], "->", script.Path);
			if (tokens[2].Value != ":")
				throw LsnrParsingException.UnexpectedToken(tokens[2], ":", script.Path);
			var cnd = substitutions[tokens[1]]; // Check if bool?
			var str = substitutions[tokens[3]]; // Check if string?
			if (str.Type != LsnType.string_.Id)
				throw new LsnrParsingException(head[0], "Improperly formatted conditional choice: The title must be an expression of type string.", script.Path);
			var p = new Parser(body, script);
			p.Parse();
			var components = Parser.Consolidate(p.Components);
			return new Choice(str, components, cnd);
		}
	}

	public sealed class CaseStructureRule : ControlStructureRule
	{
		public override int Order => ControlStructureRuleOrders.Else;

		public override bool PreCheck(Token t) => true;

		public override bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens.Last().Value == "->";

		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			// It's a choice (inside a choice block).
			var p = new Parser(body, script);
			p.Parse();
			var components = Parser.Consolidate(p.Components);
			//var endOfStr = head.IndexOf("->");

			var str = Create.Express(head.CreateSubSlice(0, head.Count - 1), script);

			return new Choice(str, components);
		}
	}
}
