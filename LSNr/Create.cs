using LsnCore;
using Tokens;
using LsnCore.ControlStructures;
using LsnCore.Expressions;
using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public static ControlStructure ControlStructure(List<IToken> head, List<IToken> body, IPreScript script)
		{
			string h = head[0].Value;
			int n = head.Count;
			if (h == "if")
			{
				var cnd = Express(head.Skip(2).Take(head.Count - 3).ToList(), script);
				script.CurrentScope = script.CurrentScope.CreateChild();
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
                script.CurrentScope = script.CurrentScope.Pop(components);
				return new IfControl(cnd, components);
			}
			if (h == "elsif")
			{
				script.CurrentScope = script.CurrentScope.CreateChild();
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new ElsifControl(Express(head.Skip(1).ToList(),script), components);
			}
			if (h == "else")
			{
				script.CurrentScope = script.CurrentScope.CreateChild();
				Parser p = new Parser(body,script);
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
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				return new ChoicesBlockControl(components);
			}
			if(h == "when")
			{ /* when [expression (condition)] : [expression] -> [block] */
				// It's a conditional choice (inside a choice block).
				int endOfCondition = head.ToList().IndexOf(":");
				var cnd = Express(head.Skip(1).Take(endOfCondition - 1),script);
				var endOfStr = head.IndexOf("->");
				var str = Express(head.Skip(endOfCondition).Take(endOfStr - endOfCondition), script);
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				return new Choice(str, components, cnd);
			}
			if (h == "for")
			{
				script.CurrentScope = script.CurrentScope.CreateChild();
				if (head.Count < 10)
					throw new ApplicationException("Incorrect for loop head thing [incorrectness inferred by too few tokens].");
				if (head.Select(t => t.Value).Count(vl => vl == "`") != 2)
					throw new ApplicationException("Incorrectly formatted for loop...");

				if (head[1].Value != "(") throw new ApplicationException("A for loop must use parenthesis...");//[e.g \'for   (   i = 0` i < cat`i++   )   \']");

				string varName = head[2].Value; // Check if it exists, type, mutable...
				
				if (head[3].Value != "=") throw new ApplicationException("...");

				var exprTokens = new List<IToken>();
				int i = 4;
				do
				{
					exprTokens.Add(head[i]);
				} while (head[++i].Value != "`");
				// i points to `.
				var val = Express(exprTokens, script);
				if (!script.CurrentScope.HasVariable(varName)) script.CurrentScope.CreateVariable(varName, true, val);

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
						for(i = i+1; i < head.Count -1; i++)
							exprTokens.Add(head[i]);
						post = new ReassignmentStatement(script.CurrentScope.GetVariable(varName).Index, Express(exprTokens, script));
					}
					else if(head[i].Value == "++")
					{
						throw new NotImplementedException();
					}
					else if (head[i].Value == "--")
					{
						throw new NotImplementedException();
					}
				}
				else if (v == "--")
				{
					if (head[++i].Value != varName)
						throw new ApplicationException("...");
					throw new NotImplementedException();
				}
				else if(v == "++")
				{
					if (head[++i].Value != varName)
						throw new ApplicationException("...");
					throw new NotImplementedException();
				}

				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);

				return new ForLoop(varName, val, con, components, post);
			}
			if(n > 1 && head.Last().Value == "->")
			{
				// It's a choice (inside a choice block).
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				//var endOfStr = head.IndexOf("->");
				var str = Express(head.Take(n - 1), script);

				return new Choice(str, components);
            }
			return null;
		}
		

		private static IExpression Express(IEnumerable<IToken> tokens, IPreScript script/*, IExpressionContainer container*/)
			=> Express(tokens.ToList(), script);

		private static List<Variable> __variables = new List<Variable>();

		/// <summary>
		/// Create an expression.
		/// </summary>
		/// <param name="list"> The list of tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static IExpression Express(List<IToken> list, IPreScript script, IReadOnlyDictionary<IToken,IExpression> substitutions = null)
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
			if(substitutions == null)
				return ExpressionBuilder.Build(list, script);
			return ExpressionBuilder.Build(list, script, substitutions, substitutions.Count);
        }

		public static IExpression SingleTokenExpress(IToken token, IPreScript script, IExpressionContainer container = null, IList<Variable> variables = null)
		{
			var val = token.Value;
			var symType = script.CheckSymbol(val);
			IExpression expr;
			switch (symType)
			{
				case SymbolType.Variable:
					var v = script.CurrentScope.GetVariable(val);
					if (!v.Mutable && (v.InitialValue?.IsReifyTimeConst() ?? false))
						return v.InitialValue.Fold();
					expr = v.GetAccessExpression();//new VariableExpression(v.Name, v.Type);
					if (container != null)
						v.AddUser(container);
					else
						variables?.Add(v);
					return expr;
				case SymbolType.GlobalVariable:
					throw new NotImplementedException();
				case SymbolType.Field:
					throw new NotImplementedException();
				case SymbolType.Property:
					var preScrFn = script as PreScriptObjectFunction;
					var preScr = preScrFn.Parent;
					IExpression scrObjExpr = new VariableExpression(0, preScr.Id);
					expr = new PropertyAccessExpression(new VariableExpression(0, preScr.Id), preScr.GetPropertyIndex(val), preScr.GetProperty(val).Type);
					return expr;
				default:
					break;
			}
			if (script.CurrentScope.VariableExists(val))
			{
				
			}
			var t = token as Token;
			if(t != null)
			{
				switch (t.Type)
				{
					case TokenType.Float:
						return new LsnValue(t.DoubleValue);
					case TokenType.Integer:
						return new LsnValue(t.IntValue);
					case TokenType.String:
						return new LsnValue(new StringValue(t.Value));
					case TokenType.Substitution:
						throw new ApplicationException();
					default:
						break;
				}
			}
			
			if (val == "true")
				return LsnBoolValue.GetBoolValue(true);
			if (val == "false")
				return LsnBoolValue.GetBoolValue(false);
			var preScObjFn = script as PreScriptObjectFunction;
			if (val == "this")
			{
				if (preScObjFn == null)
					throw new ApplicationException("Cannot use 'this' outside a script object method or event listener.");
				return new VariableExpression(0, preScObjFn.Parent.Id);
			}
			if (val == "host")
			{
				if (preScObjFn == null)
					throw new ApplicationException("Cannot use 'host' outside a script object method or event listener.");
				return new HostInterfaceAccessExpression(new VariableExpression(0, preScObjFn.Parent.Id), preScObjFn.Parent.HostType.Id);
			}

			

			throw new ApplicationException(); //return null;
		}


		private static Expression CreateGet(List<IToken> tokens, IPreScript script)
		{
			throw new NotImplementedException();
		}

		public static IList<Tuple<string,IExpression>> CreateParamList(List<IToken> tokens, int paramCount, IPreScript script, IReadOnlyDictionary<IToken,IExpression> substitutions)
		{
			var ls = new List<Tuple<string, IExpression>>();
			var parameters = new List<List<IToken>>();
			parameters.Add(new List<IToken>());
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
					++end;
				}

				for (int i = start; i < end; i++)
				{
					if (tokens[i].Value == ",")
					{
						if (lPCount == rPCount && lBCount == rBCount)
						{ // This is not inside a nested function or index thing.
							parameters.Add(new List<IToken>());
							++paramIndex;
						}
						else // This is inside a nested function or index thing.
							parameters[paramIndex].Add(tokens[i]);
					}
					else if (tokens[i].Value == "(")
					{
						++lPCount;
						parameters[paramIndex].Add(tokens[i]);
					}
					else if (tokens[i].Value == ")")
					{
						++rPCount;
						parameters[paramIndex].Add(tokens[i]);
					}
					else if (tokens[i].Value == "[")
					{
						++lBCount;
						parameters[paramIndex].Add(tokens[i]);
					}
					else if (tokens[i].Value == "]")
					{
						++rBCount;
						parameters[paramIndex].Add(tokens[i]);
					}
					else
						parameters[paramIndex].Add(tokens[i]);
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
						parameters.Add(new List<IToken>());
						++paramIndex;
					}
					else
						parameters[paramIndex].Add(tokens[i]);
				}
			}

			// Parse the parameters
			for (int i = 0; i < parameters.Count; i++)
			{
				var p = parameters[i];
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
		/// 
		/// </summary>
		/// <param name="tokens"> The tokens, without the function name and containing parenthesis</param>
		/// <param name="fn"></param>
		/// <returns></returns>
		public static FunctionCall CreateFunctionCall(List<IToken> tokens, Function fn, IPreScript script, IReadOnlyDictionary<IToken, IExpression> substitutions = null)
		{
			var ls = CreateParamList(tokens, fn.Parameters.Count, script, substitutions);
			return fn.CreateCall(ls);
		}

		public static MethodCall CreateMethodCall(List<IToken> tokens, Method method, IExpression obj, IPreScript script, IReadOnlyDictionary<IToken,IExpression> substitutions = null)
		{
			var ls = CreateParamList(tokens, method.Parameters.Count, script,substitutions);
			return method.CreateMethodCall(ls,obj,true/*script.TypeIsIncluded(obj.Type)*/);
		}
		
	}
}
