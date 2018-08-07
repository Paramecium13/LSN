using LsnCore;
using LsnCore.ControlStructures;
using LsnCore.Expressions;
using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSNr
{
	/// <summary>
	/// 
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
		public static ControlStructure ControlStructure(List<Token> head, List<Token> body, IPreScript script)
		{
			var h = head[0].Value;
			var n = head.Count;
			if (h == "if")
			{
				var cnd = Express(head.Skip(2).Take(head.Count - 3).ToList(), script);
				script.CurrentScope = script.CurrentScope.CreateChild();
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
                script.CurrentScope = script.CurrentScope.Pop(components);
				return new IfControl(cnd, components);
			}
			if (h == "elsif")
			{
				script.CurrentScope = script.CurrentScope.CreateChild();
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new ElsifControl(Express(head.Skip(1).ToList(),script), components);
			}
			if (h == "else")
			{
				script.CurrentScope = script.CurrentScope.CreateChild();
				var p = new Parser(body,script);
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
			if(h == "choice")
			{
				// It's a choice block.
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				return new ChoicesBlockControl(components);
			}
			if(h == "when")
			{ /* when [expression (condition)] : [expression] -> [block] */
				// It's a conditional choice (inside a choice block).
				var endOfCondition = head.ToList().IndexOf(":");
				var cnd = Express(head.Skip(1).Take(endOfCondition - 1),script);
				var endOfStr = head.IndexOf("->");
				var str = Express(head.Skip(endOfCondition).Take(endOfStr - endOfCondition), script);
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				return new Choice(str, components, cnd);
			}
			if (h == "for")
			{
				script.CurrentScope = script.CurrentScope.CreateChild();
				if (head.Count < 10)
					throw new LsnrParsingException(head[0], "Incorrectly formatted for loop.", script.Path);
				if (head.Select(t => t.Value).Count(vl => vl == "`") != 2)
					throw new LsnrParsingException(head[0], "Incorrectly formatted for loop.", script.Path);

				if (head[1].Value != "(")
					throw new LsnrParsingException(head[1], "Incorrectly formatted for loop. (A for loop must use parenthesis)", script.Path);

				var varName = head[2].Value; // Check if it exists, type, mutable...

				if (head[3].Value != "=") throw new LsnrParsingException(head[3], "Incorrectly formatted for loop.", script.Path);

				var exprTokens = new List<Token>();
				var i = 4;
				do
				{
					exprTokens.Add(head[i]);
				} while (head[++i].Value != "`");
				// i points to `.
				var val = Express(exprTokens, script);

				Variable vr;
				if (script.CurrentScope.HasVariable(varName))
				{
					vr = script.CurrentScope.GetVariable(varName);
				}
				else vr = script.CurrentScope.CreateVariable(varName, true, val);

				exprTokens.Clear(); // Recycle the list.
				i++;
				do
				{
					exprTokens.Add(head[i]);
				} while (head[++i].Value != "`");
				// i points to `.
				var con = Express(exprTokens, script);
				exprTokens.Clear();
				i++;
				var v = head[i].Value;
				Statement post = null;
				if ( v == varName)
				{
					i++;
					if(head[i].Value == "=")
					{
						for(i = i + 1; i < head.Count -1; i++)
							exprTokens.Add(head[i]);
						post = new AssignmentStatement(script.CurrentScope.GetVariable(varName).Index, Express(exprTokens, script));
					}
					else if(head[i].Value == "++")
					{
						throw new NotImplementedException(); // ToDo: Implement
					}
					else if (head[i].Value == "--")
					{
						throw new NotImplementedException(); // ToDo: Implement
					}
				}
				else if (v == "--")
				{
					if (head[++i].Value != varName)
						throw new LsnrParsingException(head[i], "Incorrectly formatted for loop.", script.Path);
					throw new NotImplementedException(); // ToDo: Implement
				}
				else if(v == "++")
				{
					if (head[++i].Value != varName)
						throw new LsnrParsingException(head[i], "Incorrectly formatted for loop.", script.Path);
					throw new NotImplementedException(); // ToDo: Implement
				}
				else
					throw new LsnrParsingException(head[i], "Incorrectly formatted for loop.", script.Path);

				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);

				return new ForLoop(vr.Index, val, con, components, post);
			}
			if(n > 1 && head.Last().Value == "->")
			{
				// It's a choice (inside a choice block).
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				//var endOfStr = head.IndexOf("->");
				var str = Express(head.Take(n - 1), script);

				return new Choice(str, components);
            }
			return null;
		}

		private static IExpression Express(IEnumerable<Token> tokens, IPreScript script/*, IExpressionContainer container*/)
			=> Express(tokens.ToList(), script);

		private static List<Variable> __variables = new List<Variable>();

		/// <summary>
		/// Create an expression.
		/// </summary>
		/// <param name="list"> The list of tokens.</param>
		/// <param name="script"> The script.</param>
		/// <param name="substitutions">todo: describe substitutions parameter on Express</param>
		/// <returns></returns>
		public static IExpression Express(List<Token> list, IPreScript script, IReadOnlyDictionary<Token,IExpression> substitutions = null)
		{
			if (list[0].Value/*.ToLower()*/ == "get")
			{
				return CreateGet(list,script);
			}
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
			if (substitutions == null)
#if LSS
				return LssParser.ExpressionParser.Parse(list.ToArray(), script);
			return LssParser.ExpressionParser.Parse(list.ToArray(), script, substitutions);
#else

				return ExpressionBuilder.Build(list, script);
			return ExpressionBuilder.Build(list, script, substitutions, substitutions.Count);
#endif
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
					return new FieldAccessExpression(new VariableExpression(0, preScrFn.Parent.Id), preScrFn.Parent.GetField(val));
				case SymbolType.Property:
					var preScr = preScrFn.Parent;
					return new PropertyAccessExpression(new VariableExpression(0, preScr.Id), preScr.GetPropertyIndex(val), preScr.GetProperty(val).Type);
			}
			if (token != null)
			{
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
				}
			}

			if (val == "true")
				return LsnBoolValue.GetBoolValue(true);
			if (val == "false")
				return LsnBoolValue.GetBoolValue(false);

			var preScObjFn = script as PreScriptClassFunction;
			if (val == "this")
			{
				if (preScObjFn == null)
					throw new LsnrParsingException(token, "Cannot use 'this' outside a script object method or event listener.", script.Path);
				return new VariableExpression(0, preScObjFn.Parent.Id);
			}
			if (val == "host")
			{
				if (preScObjFn == null)
					throw new LsnrParsingException(token, "Cannot use 'host' outside a script object method or event listener.", script.Path);
				return new HostInterfaceAccessExpression(preScObjFn.Parent.HostType.Id);
			}

			throw new LsnrParsingException(token, $"Cannot parse token '{token.Value}' as an expression.", script.Path);
		}

		private static Expression CreateGet(List<Token> tokens, IPreScript script)
		{
			throw new NotImplementedException();
		}

		public static IList<Tuple<string,IExpression>> CreateParamList(List<Token> tokens, int paramCount, IPreScript script, IReadOnlyDictionary<Token,IExpression> substitutions)
		{
			if (tokens.Count == 0)
				throw new ApplicationException();
			/*if(tokens[0].Value == "(")
			{
				if (tokens[tokens.Count - 1].Value == ")")
					tokens = tokens.Skip(1).Take(tokens.Count - 2).ToList();
				else tokens = tokens.Skip(1).ToList();
			}*/
			var ls = new List<Tuple<string, IExpression>>();
			var parameterTokens = new List<List<Token>> { new List<Token>() };
			int paramIndex = 0;
			if (tokens.Count(t => t.Value == ",") > paramCount - 1) // There is one less comma than parameter. numCommas = numParameters - 1, where numParameters > 0
			{
				int lPCount = 0;
				int rPCount = 0;
				int lBCount = 0;
				int rBCount = 0;
				int start = 1;
				int end = tokens.Count - 1;

				if(tokens[0].Value != "(")
				{
					start = 0;
					//++end; This would make it go out of bounds.
				}

				for (int i = start; i < end; i++)
				{
					if (tokens[i].Value == ",")
					{
						if (lPCount == rPCount && lBCount == rBCount)
						{ // This is not inside a nested function or index thing.
							parameterTokens.Add(new List<Token>());
							++paramIndex;
						}
						else // This is inside a nested function or index thing.
							parameterTokens[paramIndex].Add(tokens[i]);
					}
					else if (tokens[i].Value == "(")
					{
						++lPCount;
						parameterTokens[paramIndex].Add(tokens[i]);
					}
					else if (tokens[i].Value == ")")
					{
						++rPCount;
						parameterTokens[paramIndex].Add(tokens[i]);
					}
					else if (tokens[i].Value == "[")
					{
						++lBCount;
						parameterTokens[paramIndex].Add(tokens[i]);
					}
					else if (tokens[i].Value == "]")
					{
						++rBCount;
						parameterTokens[paramIndex].Add(tokens[i]);
					}
					else
						parameterTokens[paramIndex].Add(tokens[i]);
				}
			}
			else
			{
				// Split the list of tokens into multiple lists by commas.
				for (int i = 0; i < tokens.Count; i++) // Takes into account starting and closing parenthesis.
														   // (int i = 0; i < tokens.Count; i++) [old]
				{
					if (tokens[i].Value == ",")
					{
						parameterTokens.Add(new List<Token>());
						++paramIndex;
					}
					else
						parameterTokens[paramIndex].Add(tokens[i]);
				}
			}

			// Parse the parameters
			for (int i = 0; i < parameterTokens.Count; i++)
			{
				var p = parameterTokens[i];
				if (p.Count > 2 && p[1].Value == ":") // It's named
				{
					ls.Add(new Tuple<string, IExpression>(p[0].Value, Express(p.Skip(2).ToList(), script, substitutions)));
				}
				else
				{
					ls.Add(new Tuple<string, IExpression>("", Express(p, script,substitutions)));
				}
			}
			return ls;
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="tokens"> The tokens, without the function name and containing parenthesis</param>
		/// <param name="fn"></param>
		/// <param name="script">todo: describe script parameter on CreateFunctionCall</param>
		/// <param name="substitutions">todo: describe substitutions parameter on CreateFunctionCall</param>
		/// <returns></returns>
		public static FunctionCall CreateFunctionCall(List<Token> tokens, Function fn, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions = null)
		{
			var ls = CreateParamList(tokens, fn.Parameters.Count, script, substitutions);
			return fn.CreateCall(ls);
		}

		public static IExpression CreateMethodCall(List<Token> tokens, Method method, IExpression obj, IPreScript script, IReadOnlyDictionary<Token,IExpression> substitutions = null)
		{
			var ls = CreateParamList(tokens, method.Parameters.Count, script,substitutions);
			return method.CreateMethodCall(ls,obj);
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
			}

			return (argTokens.ToArray(), j + 1);
		}
	}
}
