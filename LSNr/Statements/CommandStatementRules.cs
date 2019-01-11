using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;

namespace LSNr.Statements
{
	public sealed class SayStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t) => t.Value == "say";

		public bool Check(ISlice<Token> tokens, IPreScript script) => true;

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			tokens = tokens.CreateSliceAt(1);
			IExpression message, graphic = LsnValue.Nil, title = LsnValue.Nil;
			var asIndex = tokens.IndexOf("as");
			var withIndex = tokens.IndexOf("with");
			if (withIndex < 0) withIndex = tokens.IndexOf("withgraphic");
			if (withIndex > 0)
			{
				if (asIndex > 0)
				{
					var firstIndex = Math.Min(withIndex, asIndex);
					var secondIndex = Math.Max(withIndex, asIndex);
					message = Create.Express(tokens.CreateSliceTaking(firstIndex), script);
					var expr2 = Create.Express(tokens.CreateSliceBetween(firstIndex, secondIndex), script);
					var expr3 = Create.Express(tokens.CreateSliceAt(secondIndex), script);
					if (firstIndex == withIndex) { graphic = expr2; title = expr3; }
					else { title = expr2; graphic = expr3; }
				}
				else
				{
					message = Create.Express(tokens.CreateSliceTaking(withIndex), script);
					graphic = Create.Express(tokens.CreateSliceAt(withIndex), script);
				}
			}
			else if (asIndex > 0)
			{
				message = Create.Express(tokens.CreateSliceTaking(asIndex), script);
				title = Create.Express(tokens.CreateSliceAt(asIndex), script);
			}
			else // No title or graphic
				message = Create.Express(tokens, script);

			return new SayStatement(message, graphic, title);
		}
	}

	public sealed class GoToStatementRule : IStatementRule
	{
		public int Order => throw new NotImplementedException();

		public bool PreCheck(Token t) => true;

		public bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens.Any(t => t.Value == "goto");

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			/*var res = LssParser.ExpressionParser.MultiParse(tokens, script);
			tokens = res.tokens.ToSlice();
			var subs = res.substitutions;
			if(tokens[0].Type == TokenType.Substitution)
			...*/
			IExpression actor = null;
			IReadOnlyList<Token> tokens0 = null;
			if (tokens[0].Value == "goto")
				tokens0 = tokens;
			else
			{
				var actorTokens = tokens.TakeWhile(t => t.Value != "goto").ToList();
				tokens0 = tokens.Skip(actorTokens.Count + 1).ToList();
				actor = Create.Express(actorTokens, script);
			}

			var metaCommaCount = tokens0.Count(t => t.Value == "`");
			IExpression expr0 = null;
			IExpression expr1 = null;
			IExpression expr2 = null;
			switch (metaCommaCount)
			{
				case 0:
					expr0 = Create.Express(tokens0, script);
					break;
				case 1:
					{
						var tokens1 = tokens0.TakeWhile(t => t.Value != "`").ToList();
						var tokens2 = tokens0.Skip(tokens1.Count + 1).ToList();
						expr0 = Create.Express(tokens1, script);
						expr1 = Create.Express(tokens2, script);
						break;
					}
				case 2:
					{
						var tokens1 = tokens0.TakeWhile(t => t.Value != "`").ToList();
						var tokens2 = tokens0.Skip(tokens1.Count + 1).TakeWhile(t => t.Value != "`").ToList();
						var tokens3 = tokens0.Skip(tokens1.Count + tokens2.Count + 2).ToList();
						expr0 = Create.Express(tokens1, script);
						expr1 = Create.Express(tokens2, script);
						expr2 = Create.Express(tokens3, script);
						break;
					}
				default:
					throw new LsnrParsingException(tokens.First(t => t.Value.ToLower() == "goto"), "Improperly formatted goto statement (considered harmful).", script.Path);
			}
			return new GoToStatement(expr0, expr1, expr2, actor);
		}
	}

	public sealed class AttachStatementRule : IStatementRule
	{
		public int Order => throw new NotImplementedException();

		public bool PreCheck(Token t) => t.Value == "attach";

		public bool Check(ISlice<Token> tokens, IPreScript script) => true;

		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			if (tokens[1].Value.ToLower() != "new")
				throw LsnrParsingException.UnexpectedToken(tokens[1], "'new'", script.Path);
			var scClassName = tokens[2].Value; // If generic script classes are supported, use a get type function.
			if (!script.TypeExists(scClassName))
				throw new LsnrParsingException(tokens[2], $"No type named '{scClassName}' exists.", script.Path);
			var scClassType = script.GetType(scClassName) as ScriptClass;
			if (scClassType == null)
				throw new LsnrParsingException(tokens[2], $"The type ${scClassName} is not a script class.", script.Path);

			if (scClassType.Unique)
				throw new LsnrParsingException(tokens[2], $"Cannot create an instance of a unique script class ('{scClassName}')", script.Path);

			int i = 4;
			List<Token>[] parseP(string open, string close)
			{
				var ls = new List<List<Token>>();
				var paramTokens = new List<Token>();
				int pCount = 1;
				do
				{
					if (++i >= tokens.Count)
						throw new LsnrParsingException(tokens[i - 1], "Mismatched parenthesis.", script.Path);
					var t = tokens[i];
					var x = t.Value;
					if (x == open) ++pCount; // ++lCount;
					if (x == close) --pCount; // ++rCount
					if (x == "," && pCount == 1)
					{
						ls.Add(paramTokens);
						paramTokens = new List<Token>();
					}
					else paramTokens.Add(t);
				} while (pCount != 0);
				return ls.ToArray();
			}
			List<Token>[] args = null;
			List<Token>[] props = null;
			if (tokens[3].Value == "(")
			{
				args = parseP("(", ")");
				if (tokens[i].Value != "[" && scClassType.Properties.Count != 0)
					throw LsnrParsingException.UnexpectedToken(tokens[i], "[", script.Path);
				i++;
				props = parseP("[", "]");
			}
			else if (tokens[3].Value == "[")
			{
				props = parseP("[", "]");
				if (tokens[i].Value != "(")
					throw LsnrParsingException.UnexpectedToken(tokens[i], "[", script.Path);
				i++;
				args = parseP("(", ")");
			}
			else throw LsnrParsingException.UnexpectedToken(tokens[3], "'[' or '('", script.Path);

			if (props.Length != scClassType.Properties.Count)
				throw new LsnrParsingException(tokens[2], $"All properties of script class '{scClassName}' must be given a value.",
					script.Path);

			var propExps = new IExpression[scClassType.Properties.Count];
			IExpression[] argExps = null;
			if (props.Length > 0 && props[0].Count > 2 && props[0][1].Value == ":")
			{
				if (!props.All(p => p.Count > 2 && p[1].Value == ":"))
					throw new LsnrParsingException(props[0][2],
						"Either all properties must be entered by name or all properties must be entered by position", script.Path);
				foreach (var ls in props)
				{
					var name = ls[0].Value;
					var prop = scClassType.Properties.FirstOrDefault(p => p.Name == name);
					if (prop == null)
						throw new LsnrParsingException(ls[0],
							$"The script class '{scClassName}' does not have a property named '{name}'", script.Path);
					int index = scClassType.GetPropertyIndex(name);
					if (propExps[index] != null)
						throw new LsnrParsingException(ls[0], $"Property '{name}' cannot be given two values.", script.Path);
					var expr = Create.Express(ls.Skip(2), script);
					propExps[index] = expr;
					if (!prop.Type.Subsumes(expr.Type.Type))
						throw LsnrParsingException.TypeMismatch(ls[0], prop.Type.Name, expr.Type.Name, script.Path);
				}
			}
			else
			{
				for (int j = 0; j < props.Length; j++)
				{
					var expr = Create.Express(props[j], script);
					if (!scClassType.Properties[j].Type.Subsumes(expr.Type.Type))
						throw LsnrParsingException.TypeMismatch(props[j][0], scClassType.Properties[j].Type.Name,
							expr.Type.Name, script.Path);
					propExps[j] = expr;
				}
			}
			var useCstor = scClassType.Constructor != null;

			if (useCstor && args.Length != scClassType.Constructor.Parameters.Length - 1)
				throw new LsnrParsingException(tokens[2],
					$"Incorrect number of arguments for constructor of script class '{scClassName}'.", script.Path);
			else if (args.Length != scClassType.FieldsB.Count)
				throw new LsnrParsingException(tokens[2],
					$"Incorrect number of fields for script class '{scClassName}'.", script.Path);

			if (args.Length > 0 && args[0].Count > 2 && args[0][1].Value == ":")
			{
				if (!args.All(p => p.Count > 2 && p[1].Value == ":"))
				{
					var x = useCstor ? "arguments" : "fields";
					throw new LsnrParsingException(props[0][2],
						$"Either all {x} must be entered by name or all {x} must be entered by position", script.Path);
				}
				foreach (var ls in args)
				{
					var name = ls[0].Value;
					int index;
					var expr = Create.Express(ls.Skip(2), script);
					if (useCstor)
					{
						var param = scClassType.Constructor.Parameters.FirstOrDefault(p => p.Name == name);
						if (param == null)
							throw new LsnrParsingException(ls[0],
								$"The constructor for script class '{scClassName}' does not have an argument '{name}'.", script.Path);
						index = param.Index - 1;
						if (argExps[index] != null)
							throw new LsnrParsingException(ls[0], $"Argument '{name}' cannot be given two values.", script.Path);
						if (!param.Type.Subsumes(expr.Type.Type))
							throw LsnrParsingException.TypeMismatch(ls[0], param.Type.Name, expr.Type.Name, script.Path);
					}
					else
					{
						if (!scClassType.FieldsB.Any(f => f.Name == name))
							throw new LsnrParsingException(ls[0], $"Field '{name}' cannot be given two values.", script.Path);
						var field = scClassType.FieldsB.First(f => f.Name == name);
						index = field.Index;
						if (argExps[index] != null)
							throw new LsnrParsingException(ls[0], $"Field '{name}' cannot be given two values.", script.Path);
						if (!field.Type.Subsumes(expr.Type.Type))
							throw LsnrParsingException.TypeMismatch(ls[0], field.Type.Name, expr.Type.Name, script.Path);
					}
					argExps[index] = expr;
				}
			}
			else
			{
				for (int j = 0; j < args.Length; j++)
				{
					var expr = Create.Express(args[j], script);
					if (useCstor && !scClassType.Constructor.Parameters[j + 1].Type.Subsumes(expr.Type.Type))
						throw LsnrParsingException.TypeMismatch(args[j][0], scClassType.Constructor.Parameters[j + 1].Type.Name,
							expr.Type.Name, script.Path);
					if (!scClassType.Fields[j].Type.Subsumes(expr.Type.Type))
						throw LsnrParsingException.TypeMismatch(args[j][0], scClassType.Fields[j].Name, expr.Type.Name, script.Path);
					argExps[j] = expr;
				}
			}

			if (tokens[++i].Value != "to")
				throw LsnrParsingException.UnexpectedToken(tokens[i], "to", script.Path);
			i++;

			var host = Create.Express(tokens.Skip(i + 1), script);

			return new AttachStatement(scClassType.Id, propExps, argExps, host);
		}
	}
}
