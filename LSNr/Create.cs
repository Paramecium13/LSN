﻿using LSN_Core;
using LSN_Core.Compile;
using LSN_Core.Compile.Tokens;
using LSN_Core.ControlStructures;
using LSN_Core.Expressions;
using LSN_Core.Statements;
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
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
                script.CurrentScope = script.CurrentScope.Pop(components);
				return new IfControl(Express(head.Skip(1).ToList(), script), components);
			}
			if (h == "elsif")
			{
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new ElsifControl(Express(head.Skip(1).ToList(),script), components);
			}
			if (h == "else")
			{
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body,script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new ElseControl(Express(head.Skip(1).ToList(), script), components);
			}
			if(h == "choice")
			{
				// It's a choice block.
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				return new ChoicesBlockControl(components);
			}
			if(h == "?")
			{ /* ? [expression (condition)] ? [expression] -> [block] */
				// It's a conditional choice (inside a choice block).
				int endOfCondition = head.Skip(1).ToList().IndexOf("?");
				var cnd = Express(head.Skip(1).Take(endOfCondition - 1),script);
				var endOfStr = head.IndexOf("->");
				var str = Express(head.Skip(endOfCondition).Take(endOfStr - endOfCondition), script);
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				return new Choice(str, components, cnd);
			}
			if(n > 1 && head.Last().Value == "->")
			{
				// It's a choice (inside a choice block).
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				var endOfStr = head.IndexOf("->");
				var str = Express(head.Take(n - 1), script);

				return new Choice(str, components);
            }
			return null;
		}
		

		private static IExpression Express(IEnumerable<IToken> tokens, IPreScript script)
			=> Express(tokens.ToList(), script);

		/// <summary>
		/// Create an expression.
		/// </summary>
		/// <param name="list"> The list of tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static IExpression Express(List<IToken> list, IPreScript script)
		{
			if (list[0].Value.ToLower() == "get")
			{
				return CreateGet(list,script);
			}
			else if(list.Count == 1)
			{
				if(script.CurrentScope.VariableExists(list[0].Value))
				{
					var v = script.CurrentScope.GetVariable(list[0].Value);
					return new VariableExpression(v.Name, v.Type);
				}
				else if (list[0].GetType() == typeof(FloatToken))
				{
					return new DoubleValue(((FloatToken)list[0]).DVal);
				}
				else if (list[0].GetType() == typeof(IntToken))
				{
					return new IntValue(((IntToken)list[0]).IVal);
				}
				else if (list[0].GetType() == typeof(StringToken))
				{
					return new StringValue(list[0].Value);
				}
				else
				{
					Console.WriteLine($"Cannot parse expression {list[0]}.");
					script.Valid = false;
					return null; // Use a "Null expression" here instead so the reifier can check for more errors.
				}
			}
			else
			{
				return Compound(list, script);
			}
		}

		/// <summary>
		/// Create a compound expression.
		/// </summary>
		/// <param name="list"> The list of tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns> ...</returns>
		private static CompoundExpression Compound(List<IToken> list, IPreScript script)
		{
			const string sub = "Σ";
			int subCount = 0;
			var dict = new Dictionary<IToken, ComponentExpression>();
			var vars = new List<Variable>();
			var ls = new List<IToken>();

			for(int i = 0; i < list.Count; i++)
			{
				var val = list[i].Value;
				if (script.CurrentScope.VariableExists(val))
				{
					var v = script.CurrentScope.GetVariable(val);
					var name = sub + subCount++;
					vars.Add(v);
					dict.Add(new Identifier(name), new VariableExpression(val, v.Type));
					ls.Add(new Identifier(name));
				}
				else if(script.FunctionExists(val)) // It's the start of a function call.
				{
					if (list[i + 1].Value != "(") return null; // Or throw or log something...
					i += 2;  // Move to the right twice, now looking at token after the opening '('.
					int lCount = 1;
					int rCount = 0;
					var fnTokens = new List<IToken>();
					while(lCount != rCount)
					{
						if(list[i].Value == ")")
						{
							lCount++;
							if (lCount == rCount) break;
						}
						else if(list[i].Value == "(")
						{
							rCount++;
						}
						fnTokens.Add(list[i]);
						i++;
					}
					// Create the function call, add it to the dictionary, and add its identifier to the list (ls).
					var name = sub + subCount++;
					var fnCall = CreateFunctionCall(fnTokens, script.GetFunction(val), script);
					dict.Add(new Identifier(name), fnCall);
					ls.Add(new Identifier(name));
				}
				else
				{
					ls.Add(list[i]);
				}
			}

			var expr = new CompoundExpression(list, dict);
			foreach (var v in vars) v.Users.Add(expr);
			return expr;
		}

		private static Expression CreateGet(List<IToken> tokens, IPreScript script)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"> The tokens, without the function name and containing parenthesis</param>
		/// <param name="fn"></param>
		/// <returns></returns>
		private static FunctionCall CreateFunctionCall(List<IToken> tokens, Function fn, IPreScript script)
		{
			var ls = new List<Tuple<string, IExpression>>();
			var parameters = new List<List<IToken>>();
			parameters.Add(new List<IToken>());
			int paramIndex = 0;

			// Split the list of tokens into multiple lists by commas.
			for (int i = 0; i < tokens.Count; i++)
			{
				if(tokens[i].Value == ",")
				{
					parameters.Add(new List<IToken>());
					++paramIndex;
					continue;
				}
				parameters[paramIndex].Add(tokens[i]);
			}
			for(int i = 0; i < parameters.Count; i++)
			{
				var p = parameters[i];
				if(p.Count > 2 && p[1].Value == "=") // It's named
				{
					ls.Add(new Tuple<string, IExpression>(p[0].Value , Express(p.Skip(2).ToList(), script) ) );
				}
				else
				{
					ls.Add(new Tuple<string, IExpression>("", Express(p, script) ) );
				}
			}
			return fn.CreateCall(ls);
		}

		
	}
}
