using LSN_Core;
using LSN_Core.Expressions;
using LSN_Core.Compile;
using LSN_Core.Compile.Tokens;
using LSN_Core.Types;
using LSN_Core.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class ExpressionBuilder
	{
		const string SUB = "Σ";

		private Dictionary<IToken, IExpression> Substitutions = new Dictionary<IToken, IExpression>();
		private readonly List<IToken> InitialTokens;
		private List<IToken> CurrentTokens = new List<IToken>();
		private int SubCount = 0;
		private readonly IPreScript Script;

		/// <summary>
		/// The variables used in this expression.
		/// </summary>
		private List<Variable> Variables = new List<Variable>();

		private ExpressionBuilder(List<IToken> tokens, IPreScript script)
		{
			InitialTokens = tokens;
			Script = script;
		}


		private ExpressionBuilder(List<IToken> tokens, IPreScript script, Dictionary<IToken, IExpression> subs, int count)
		{
			InitialTokens = tokens;
			Script = script;
			Substitutions = subs;
			SubCount = count;
			
		}

		private IExpression Parse()
		{
			ParseVariablesAndFunctions();
			ParseParenthesis();
			//ParseInexers();// Put this with functions and variables?
			if (CurrentTokens.Any(t => t.Value == "^")) ParseExponents();
			if (CurrentTokens.Any(t => { var v = t.Value; return v == "*" || v == "/" || v == "%"; }))
				ParseMultDivMod();
			if (CurrentTokens.Any(t => { var v = t.Value; return v == "+" || v == "-"; }))
				ParseAddSubtract();
			ParseComparisons();
			if(CurrentTokens.Count != 1) { throw new ApplicationException("This should not happen."); }
			var expr = Substitutions[CurrentTokens[0]];
			foreach (var v in Variables) v.Users.Add(expr);
			return expr;
		}

		/// <summary>
		/// Parse variables, functions, methods, and fields.
		/// </summary>
		private void ParseVariablesAndFunctions()
		{
			for (int i = 0; i < InitialTokens.Count; i++)
			{
				var val = InitialTokens[i].Value;
				#region .
				if (val == ".")
				{
					if (i == 0)
						throw new ApplicationException("An expresion cannot start with \'.\'.");
					if (i + 1 > +InitialTokens.Count)
						throw new ApplicationException("An expresion cannot end with \'.\'.");
					IExpression expr;

					int nextIndex = i + 1; // This is the default next index.
					// Get the expression for the object calling the method or accessing a member
					if (InitialTokens[i - 1].Value == ")")
					{
						// I'm not too sure this will work...
						var tokens = new List<IToken>();
						int rightCount = 1;
						int leftCount = 0;
						int j = i - 1;
						while(true)
						{
							if (j < 0) throw new ApplicationException("Mismatched parenthesis.");
							var v = InitialTokens[j].Value;
							CurrentTokens.RemoveAt(CurrentTokens.Count - 1); // Remove the token at the end of the list.
							if (v == ")") rightCount++;
							else if (v == "(") leftCount++;
							if (leftCount == rightCount) break;
							tokens.Add(InitialTokens[j]);
							j--;
						}
						expr = Build(tokens, Script);
					}
					else
					{
						expr = GetExpression(InitialTokens[i - 1]);
					}

					var name = InitialTokens[i + 1].Value;
					IExpression expr2 = null;
					// Is it a method call or a field access expression?
					if(expr.Type.Methods.ContainsKey(name)) // It's a method call.
					{
						var method = expr.Type.Methods[name];
						if (method.Parameters.Count == 0)
							expr2 = method.CreateMethodCall
								(new List<Tuple<string, IExpression>>(), expr, false/*Script.MethodIsIncluded(name)*/);
						else
						{
							int lCount = 1;
							int rCount = 0;
							int j = 2; // Move to the right twice, now looking at token after the opening '('.
							var fnTokens = new List<IToken>();
							while (lCount != rCount)
							{
								if (InitialTokens[i + j].Value == ")")
								{
									rCount++;
									if (lCount == rCount) break;
								}
								else if (InitialTokens[i + j].Value == "(")
								{
									lCount++;
								}
								fnTokens.Add(InitialTokens[i + j]);
								j++;
							}
							nextIndex += j;
							expr2 = Create.CreateMethodCall(fnTokens, method, Script);
						}
					}
					else if (expr.Type.GetType().IsAssignableFrom(typeof(IHasFieldsType))) // It's a field access expression.
					{
						var type = (IHasFieldsType)expr.Type;
						if (!type.Fields.ContainsKey(name))
							throw new ApplicationException($"The type {expr.Type.Name} does not have a field named {name}.");
						expr2 = new FieldAccessExpression(expr, name, type.Fields[name]);
					}
					else
						throw new ApplicationException($"The type {expr.Type.Name} does not have a method named {name}.");
					
					var sub = SUB + SubCount++;
					Substitutions.Add(new Identifier(sub), expr2);
					CurrentTokens.Add(new Identifier(sub));
					i = nextIndex -1; // In the next iteration, i == nextIndex.
				}
				#endregion
				else if (Script.CurrentScope.VariableExists(val))
				{
					var v = Script.CurrentScope.GetVariable(val);
					var name = SUB + SubCount++;
					Variables.Add(v);
					Substitutions.Add(new Identifier(name), new VariableExpression(val, v.Type));
					CurrentTokens.Add(new Identifier(name));
				}
				#region Function
				else if (Script.FunctionExists(val)) // It's the start of a function call.
				{
					var fn = Script.GetFunction(val);
					int nextIndex = i + 1; // This is the default next index.
					if (InitialTokens[i + 1].Value != "(")
					{
						if(fn.Parameters.Count != 0)
							throw new ApplicationException(); // Or throw or log something...
						var fnCall = fn.CreateCall(new List<Tuple<string, IExpression>>());
						var name = SUB + SubCount++;
						Substitutions.Add(new Identifier(name), fnCall);
						CurrentTokens.Add(new Identifier(name));
					}
					else
					{
						int lCount = 1;
						int rCount = 0;
						int j = 2; // Move to the right twice, now looking at token after the opening '('.
						var fnTokens = new List<IToken>();
						while (lCount != rCount)
						{
							if (InitialTokens[i + j].Value == ")")
							{
								rCount++;
								if (lCount == rCount) break;
							}
							else if (InitialTokens[i + j].Value == "(")
							{
								lCount++;
							}
							fnTokens.Add(InitialTokens[i + j]);
							j++;
						}
						nextIndex += j;
						// Create the function call, add it to the dictionary, and add its identifier to the list (ls).
						var name = SUB + SubCount++;
						var fnCall = Create.CreateFunctionCall(fnTokens, fn, Script);
						Substitutions.Add(new Identifier(name), fnCall);
						CurrentTokens.Add(new Identifier(name));
					}
					i = nextIndex -1; // In the next iteration, i == nextIndex.
				}
				#endregion
				else
				{
					CurrentTokens.Add(InitialTokens[i]);
				}
			}
		}


		private void ParseParenthesis()
		{
			var newTokens = new List<IToken>();
			for(int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if(val == "(")
				{
					int nextIndex = i + 1;
					int lCount = 1;
					int rCount = 0;
					int j = 1;
					List<IToken> pTokens = new List<IToken>();
					while(lCount != rCount)
					{
						if (CurrentTokens[i + j].Value == ")")
							rCount++;
						else if (CurrentTokens[i + j].Value == "(")
							lCount++;
						if (lCount == rCount) break;
						pTokens.Add(CurrentTokens[i + j]);
						j++;
					}
					var expr = Build(pTokens, Script, Substitutions.Where(p => pTokens.Contains(p.Key)).ToDictionary(),SubCount);
					var name = SUB + SubCount++;
					Substitutions.Add(new Identifier(name), expr);
					newTokens.Add(new Identifier(name));
					nextIndex += j;
					i = nextIndex - 1; // In the next iteration, i == nextIndex.
				}
				else
				{
					newTokens.Add(CurrentTokens[i]);
				}
			}
			CurrentTokens = newTokens;
		}


		private void ParseMultDivMod()
		{
			var newTokens = new List<IToken>();
			for(int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if (val.Length > 2) continue;
				LSN_Core.Operator op = LSN_Core.Operator.Add;
				if (val == "*")
					op = LSN_Core.Operator.Multiply;
				else if(val == "/")
					op = LSN_Core.Operator.Divide;
				else if (val == "%")
					op = LSN_Core.Operator.Mod;

				if(op != LSN_Core.Operator.Add)
				{
					IExpression left = GetExpression(CurrentTokens[i - 1]);
					IExpression right = GetExpression(CurrentTokens[i + 1]);
					var key = new Tuple<LSN_Core.Operator, LSN_Type>(op, right.Type);

					if (!left.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator {val} is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Operators[key];
					IExpression expr = new BinaryExpression(left, right, opr.Item1, opr.Item2);
					var name = SUB + SubCount++;
					newTokens.RemoveAt(newTokens.Count - 1);
					Substitutions.Add(new Identifier(name), expr);
					newTokens.Add(new Identifier(name));
					i++; // This skips to the token after the right hand side of this expression.
				}
				else newTokens.Add(CurrentTokens[i]);
				
			}
			CurrentTokens = newTokens;
		}


		private void ParseExponents()
		{
			var newTokens = new List<IToken>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				if (CurrentTokens[i].Value == "^")
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					var key = new Tuple<LSN_Core.Operator, LSN_Type>(LSN_Core.Operator.Power, right.Type);

					if (!left.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator ^ is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Operators[key];
					IExpression expr = new BinaryExpression(left, right, opr.Item1, opr.Item2);
					var name = SUB + SubCount++;
					newTokens.RemoveAt(newTokens.Count - 1);
					Substitutions.Add(new Identifier(name), expr);
					newTokens.Add(new Identifier(name));
					i++; // This skips to the token after the right hand side of this expression.
				}
				else newTokens.Add(CurrentTokens[i]);
			}
			CurrentTokens = newTokens;
		}


		private void ParseAddSubtract()
		{
			var newTokens = new List<IToken>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				LSN_Core.Operator op = LSN_Core.Operator.Multiply;
				if (val == "+") op = LSN_Core.Operator.Add;
				else if (val == "-") op = LSN_Core.Operator.Subtract;

				if(op != LSN_Core.Operator.Multiply)
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					var key = new Tuple<LSN_Core.Operator, LSN_Type>(op, right.Type);

					if (!left.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator {val} is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Operators[key];
					IExpression expr = new BinaryExpression(left, right, opr.Item1, opr.Item2);
					var name = SUB + SubCount++;
					newTokens.RemoveAt(newTokens.Count - 1);
					Substitutions.Add(new Identifier(name), expr);
					newTokens.Add(new Identifier(name));
					i++; // This skips to the token after the right hand side of this expression.
				}
				else newTokens.Add(CurrentTokens[i]);
			}
			CurrentTokens = newTokens;
		}


		private void ParseComparisons()
		{
			var newTokens = new List<IToken>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if (val.Length > 2) continue;
				LSN_Core.Operator op = LSN_Core.Operator.Multiply;
				if (val == "<") op = LSN_Core.Operator.LessThan;
				else if (val == ">") op = LSN_Core.Operator.GreaterThan;
				else if (val == "==") op = LSN_Core.Operator.Equals;
				else if (val == ">=") op = LSN_Core.Operator.GTE;
				else if (val == "<=") op = LSN_Core.Operator.LTE;

				if (op != LSN_Core.Operator.Multiply)
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					var key = new Tuple<LSN_Core.Operator, LSN_Type>(op, right.Type);

					if (!left.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator {val} is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Operators[key];
					IExpression expr = new BinaryExpression(left, right, opr.Item1, opr.Item2);
					var name = SUB + SubCount++;
					newTokens.RemoveAt(newTokens.Count - 1);
					Substitutions.Add(new Identifier(name), expr);
					newTokens.Add(new Identifier(name));
					i++; // This skips to the token after the right hand side of this expression.
				}
				else newTokens.Add(CurrentTokens[i]);
			}
			CurrentTokens = newTokens;
		}


		private IExpression GetExpression(IToken token)
		{
			if (Substitutions.ContainsKey(token)) return Substitutions[token];
			var type = token.GetType();
            if (type == typeof(FloatToken)) // It's a double literal.
				return new DoubleValue(((FloatToken)token).DVal);
			if (type == typeof(IntToken)) // It's an integer literal.
				return new IntValue(((IntToken)token).IVal);
			if (type == typeof(StringToken)) // It's a string literal.
				return new StringValue(token.Value);
			return null;
		}

		public static IExpression Build(List<IToken> tokens, IPreScript script)
		{
			var b = new ExpressionBuilder(tokens, script);
			return b.Parse();
		}


		private static IExpression Build(List<IToken> tokens, IPreScript script, Dictionary<IToken,IExpression> subs, int i)
		{
			var b = new ExpressionBuilder(tokens, script, subs, i);
			return b.Parse();
		}

	}
}
