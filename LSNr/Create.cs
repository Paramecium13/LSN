using LsnCore;
using LsnCore.ControlStructures;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;
using LsnCore.Values;
using LSNr.LssParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSNr
{
	/// <summary>
	/// ...
	/// </summary>
	static partial class Create
	{
		/// <summary>
		/// Creates a control structure.
		/// </summary>
		/// <param name="head"> The head tokens.</param>
		/// <param name="body"> The body tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static ControlStructure ControlStructure(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var first = head[0];
			foreach (var rule in script.ControlStructureRules)
			{
				if (rule.PreCheck(first) && rule.Check(head, script))
					return rule.Apply(head, body, script);
			}
			throw new LsnrParsingException(first, "Not a valid control structure.", script.Path);
		}

		/// <summary>
		/// Creates a control structure.
		/// </summary>
		/// <param name="head"> The head tokens.</param>
		/// <param name="body"> The body tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static ControlStructure ControlStructure_Old(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var h = head[0].Value;
			var n = head.Count;
			if (h == "if" && (n > 1 && head[1].Value == "let"))
			{
				// if  let  x  =  ...    {
				// 0   1    2  3  [4,n)  n
				if (n < 5 || head[3].Value != "=")
					throw new LsnrParsingException(head[1], "improperly formatted 'if let' structure.", script.Path);
				if (head[2].Value == "mut" && head[3].Value != "=")
					throw new LsnrParsingException(head[2], "cannot declare the variable of an if let structure as mutable.", script.Path);
				var vName = head[2].Value;
				var exprTokens = head.CreateSliceAt(4);
				var expr = Express(exprTokens, script, null);
				var exprType = expr.Type.Type as OptionType;
				if (exprType == null)
					throw new LsnrParsingException(head[4], "The value of an if let structure must be of an option type.", script.Path);

				script.CurrentScope = script.CurrentScope.CreateChild();
				Variable variable;
				AssignmentStatement assignment = null;
				if (ForInCollectionLoop.CheckCollectionVariable(expr))
				{
					variable = script.CurrentScope.CreateMaskVariable(vName, new HiddenCastExpression(expr, exprType.Contents), exprType.Contents.Type);
				}
				else
				{
					variable = script.CurrentScope.CreateVariable(vName, exprType.Contents.Type);
					assignment = new AssignmentStatement(variable.Index, new HiddenCastExpression(expr, exprType.Contents));
					variable.Assignment = assignment;
				}
				var p = new Parser(body, script);
				p.Parse();
				var components = new List<Component>();
				if (assignment != null) components.Add(assignment);
				components.AddRange(Parser.Consolidate(p.Components));

				return new IfControl(expr, components);
			}
			if (h == "if")
			{
				var cnd = Express(head.Skip(1).ToList(), script);
				script.CurrentScope = script.CurrentScope.CreateChild();
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new IfControl(cnd, components);
			}
			if (h == "elsif" || (h == "else" && head.Count > 2 && head[1].Value == "if"))
			{
				var offset = h == "else" ? 1 : 0;
				script.CurrentScope = script.CurrentScope.CreateChild();
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new ElsifControl(Express(head.Skip(1 + offset).ToList(), script), components);
			}
			if (h == "else")
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
			if (h == "choice")
			{
				// It's a choice block.
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				return new ChoicesBlockControl(components);
			}
			if (h == "when")
			{ /* when [expression (condition)] : [expression] -> [block] */
			  // It's a conditional choice (inside a choice block).
				var res = ExpressionParser.MultiParse(head.ToArray(), script);
				// {"when", condition, :, title, ->}
				// validate
				if (res.tokens.Length != 5 || res.tokens[1].Type != TokenType.Substitution || res.tokens[3].Type != TokenType.Substitution)
					throw new LsnrParsingException(head[0], "Improperly formatted conditional choice.\r\n Proper format: when 'cond' : 'label' -> {...", script.Path);
				if (res.tokens[4].Value != "->")
					//throw new LsnrParsingException(head[0], "Improperly formatted conditional choice: must end in '->'.", script.Path);
					throw LsnrParsingException.UnexpectedToken(res.tokens[4], "->", script.Path);
				if (res.tokens[2].Value != ":")
					throw LsnrParsingException.UnexpectedToken(res.tokens[2], ":", script.Path);
				var cnd = res.substitutions[res.tokens[1]]; // Check if bool?
				var str = res.substitutions[res.tokens[3]]; // Check if string?
				if (str.Type != LsnType.string_.Id)
					throw new LsnrParsingException(head[0], "Improperly formatted conditional choice: The title must be an expression of type string.", script.Path);
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				return new Choice(str, components, cnd);
			}
			if (h == "for")
			{
				var i = 1;
				if (head.Length < 4 || head[i].Type != TokenType.Identifier)
					throw new LsnrParsingException(head[0], "Incorrectly formatted for loop.", script.Path);
				var vName = head[i].Value;
				if (script.CurrentScope.HasVariable(vName))
					throw new LsnrParsingException(head[1], $"A variable named '{vName}' already exists. Cannot reuse it in for loop even if it is marked as 'mut'", script.Path);
				if (head[++i].Value != "in") // i == 2
					throw LsnrParsingException.UnexpectedToken(head[i], "in", script.Path);
				i++; // i == 3; points to expression.
				var expr = Express(head.Skip(3), script);
				if (expr.Type.Type is ICollectionType collType)
				{
					script.CurrentScope = script.CurrentScope.CreateChild();
					var index = script.CurrentScope.CreateVariable(vName + " index", LsnType.int_);
					IExpression collection;
					AssignmentStatement state = null;
					if (ForInCollectionLoop.CheckCollectionVariable(expr))
						collection = expr;
					else
					{
						var collVar = script.CurrentScope.CreateVariable(vName + " collection", expr.Type.Type);
						collection = collVar.AccessExpression;
						state = new AssignmentStatement(collVar.Index, expr);
						collVar.Assignment = state;
						collVar.AddUser(state);
						collVar.MarkAsUsed();
					}
					var iterator = script.CurrentScope.CreateIteratorVariable(vName, collection, index);
					iterator.MarkAsUsed();
					var p = new Parser(body, script);
					p.Parse();
					var components = Parser.Consolidate(p.Components);
					script.CurrentScope = script.CurrentScope.Pop(components);
					return new ForInCollectionLoop(index, iterator, collection, components)
					{ Statement = state };
				}
				if (expr.Type.Type == RangeType.Instance)
				{
					script.CurrentScope = script.CurrentScope.CreateChild();
					var index = script.CurrentScope.CreateVariable(vName, LsnType.int_);
					var p = new Parser(body, script);
					p.Parse();
					var components = Parser.Consolidate(p.Components);
					script.CurrentScope = script.CurrentScope.Pop(components);
					var loop = new ForInRangeLoop(index, components);
					switch (expr)
					{
						case RangeExpression rExp:
							if (rExp.Start is LsnValue v1)
							{
								loop.Start = new LsnValue(v1.IntValue);
								// statement for end...
								var endVar = script.CurrentScope.CreateVariable("# " + vName, LsnType.int_);
								endVar.MarkAsUsed();
								var st1 = new AssignmentStatement(endVar.Index, rExp.End);
								endVar.Assignment = st1;
								loop.Statement = st1;
								endVar.AddUser(st1);
								loop.End = endVar.AccessExpression;
							}
							else if (rExp.End is LsnValue v2)
							{
								loop.End = v2;
								loop.Start = rExp.Start;
							}
							else goto default;
							break;
						case LsnValue val:
							var range = val.Value as RangeValue;
							loop.Start = new LsnValue(range.Start);
							loop.End = new LsnValue(range.End);
							break;
						case VariableExpression v:
							loop.Start = new FieldAccessExpression(v, 0);
							loop.End = new FieldAccessExpression(v, 1);
							v.Variable.AddUser(loop.Start);
							v.Variable.AddUser(loop.End);
							break;
						default:
							var rVar = script.CurrentScope.CreateVariable("# " + vName, RangeType.Instance);
							rVar.MarkAsUsed();
							var st = new AssignmentStatement(rVar.Index, expr);
							rVar.AddUser(st);
							rVar.Assignment = st;
							loop.Statement = st;

							loop.Start = new FieldAccessExpression(rVar.AccessExpression, 0);
							loop.End = new FieldAccessExpression(rVar.AccessExpression, 1);
							rVar.AddUser(loop.Start);
							rVar.AddUser(loop.End);
							break;
					}
					return loop;
				}
				else throw new LsnrParsingException(head[3], "...", script.Path);
			}
			if (n > 1 && head.Last().Value == "->")
			{
				// It's a choice (inside a choice block).
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				//var endOfStr = head.IndexOf("->");

				var str = Express(head.CreateSubSlice(0, n - 1), script);

				return new Choice(str, components);
			}
			return null;
		}

		public static IExpression Express(IEnumerable<Token> tokens, IPreScript script/*, IExpressionContainer container*/)
			=> Express(tokens.ToList(), script);

		private static List<Variable> __variables = new List<Variable>();

		/// <summary>
		/// Create an expression.
		/// </summary>
		/// <param name="list"> The list of tokens.</param>
		/// <param name="script"> The script.</param>
		/// <param name="substitutions">todo: describe substitutions parameter on Express</param>
		/// <returns></returns>
		public static IExpression Express(IReadOnlyList<Token> list, IPreScript script, IReadOnlyDictionary<Token,IExpression> substitutions = null)
		{
			if(list.Count == 1)
			{
				var token = list[0];
				if (substitutions != null && substitutions.ContainsKey(token))
					return substitutions[token];
				var expr = SingleTokenExpress(token, script, null, __variables);
				if(__variables.Count != 0)
				{
					__variables[0].AddUser(expr as IExpressionContainer);
					__variables.Clear();
				}
				return expr;
			}
			return ExpressionParser.Parse(list, script, substitutions).Fold();
		}

		public static IExpression SingleTokenExpress(Token token, IPreScript script, IExpressionContainer container = null, IList<Variable> variables = null)
		{
			var val = token.Value;
			var symType = script.CheckSymbol(val);
			IExpression expr;
			var preScrFn = script as PreScriptClassFunction;
			switch (symType)
			{
				case SymbolType.Variable:
					var v = script.CurrentScope.GetVariable(val);
					if (!v.Mutable && (v.InitialValue?.IsReifyTimeConst() ?? false))
						return v.InitialValue.Fold();
					expr = v.AccessExpression;//new VariableExpression(v.Name, v.Type);
					if (container != null)
						v.AddUser(container);
					else
						variables?.Add(v);
					return expr;
				case SymbolType.UniqueScriptObject:
					return new UniqueScriptObjectAccessExpression(val, script.GetTypeId(val));
				case SymbolType.GlobalVariable:
					throw new NotImplementedException();
				case SymbolType.Field:
					return new FieldAccessExpression(preScrFn.CurrentScope.GetVariable("self").AccessExpression,preScrFn.Parent.GetField(val));
				case SymbolType.Property:
					var preScr = preScrFn.Parent;
					return new PropertyAccessExpression(preScrFn.CurrentScope.GetVariable("self").AccessExpression, preScr.GetPropertyIndex(val), preScr.GetProperty(val).Type);
				default:
					break;
			}
			switch (token.Type)
			{
				case TokenType.Float:
					return new LsnValue(token.DoubleValue);
				case TokenType.Integer:
					return new LsnValue(token.IntValue);
				case TokenType.String:
					return new LsnValue(new StringValue(token.Value));
				case TokenType.Substitution:
					throw new ApplicationException();
				default:
					break;
			}

			switch (val)
			{
				case "true":
					return LsnBoolValue.GetBoolValue(true);
				case "false":
					return LsnBoolValue.GetBoolValue(false);
				case "self":
				{
					if (preScrFn == null)
						throw new LsnrParsingException(token, "Cannot use 'this' outside a script object method or event listener.", script.Path);
					return script.CurrentScope.GetVariable("self").AccessExpression;
				}
				case "host":
				{
					if (preScrFn == null)
						throw new LsnrParsingException(token, "Cannot use 'host' outside a script object method or event listener.", script.Path);
					return new HostInterfaceAccessExpression(preScrFn.Parent.HostType.Id);
				}
				case "none": return LsnValue.Nil;
				default:
					throw new LsnrParsingException(token, $"Cannot parse token '{token.Value}' as an expression.", script.Path);
			}
		}

		public static (Token[][] argTokens, int indexOfNextToken) CreateArgList(int indexOfOpen, IReadOnlyList<Token> tokens, IPreScript script)
		{
			var argTokens = new List<Token[]>();
			var currentList = new List<Token>();
			var j = indexOfOpen;
			if (j >= tokens.Count || tokens[j].Value != "(")
				throw new LsnrParsingException(tokens[j], "...", script.Path);

			void pop()
			{
				argTokens.Add(currentList.ToArray());
				currentList.Clear();
			}

			var balance = 1;
			while (balance != 0)
			{
				++j;
				var t = tokens[j];
				if (t.Value == "(")
				{
					++balance;
					currentList.Add(t);
				}
				else if (t.Value == ")")
				{
					--balance;
					if (balance != 0)
						currentList.Add(t);
				}
				else if (t.Value == ",")
				{
					if (balance == 1)
						pop();
					else
						currentList.Add(t);
				}
				else currentList.Add(t);
			}
			if (currentList.Count != 0)
				pop();

			return (argTokens.ToArray(), j + 1);
		}

		public static (IExpression[] args, int nextIndex)
			CreateArgs(int indexOfOpen, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions = null)
		{
			var x = CreateArgList(indexOfOpen, tokens, script);
			var args = new IExpression[x.argTokens.Length];
			for (int i = 0; i < x.argTokens.Length; i++)
				args[i] = LssParser.ExpressionParser.Parse(x.argTokens[i], script, substitutions);
			return (args, x.indexOfNextToken);
		}
	}
}
