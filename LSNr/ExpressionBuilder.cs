using LsnCore;
using LsnCore.Expressions;
using Tokens;
using Tokens.Tokens;
using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class ExpressionBuilder
	{
		const string SUB = "Ƨ";

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
			if(CurrentTokens.Count != 1)
				ParseParenthesis();
			//ParseInexers();// Put this with functions and variables?
			if (CurrentTokens.Any(t => t.Value == "^")) ParseExponents();
			if (CurrentTokens.Any(t => { var v = t.Value; return v == "*" || v == "/" || v == "%"; }))
				ParseMultDivMod();
			if (CurrentTokens.Any(t => { var v = t.Value; return v == "+" || v == "-"; }))
				ParseAddSubtract();
			ParseComparisons();
			if(CurrentTokens.Count != 1) { throw new ApplicationException("This should not happen."); }
			var expr = Substitutions[CurrentTokens[0]].Fold();
			if(! expr.IsReifyTimeConst())
				foreach (var v in Variables) v.Users.Add(expr);
			return expr;
		}

		/// <summary>
		/// Parse variables, boolean and null/nil/none/nothing literals, functions, methods, and fields.
		/// </summary>
		private void ParseVariablesAndFunctions()
		{
			for (int i = 0; i < InitialTokens.Count; i++)
			{
				var val = InitialTokens[i].Value;
				#region .
				if (val == ".") //Member Access
				{
					if (i == 0)
						throw new ApplicationException("An expresion cannot start with \'.\'.");
					if (i + 1 > InitialTokens.Count)
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
						CurrentTokens.RemoveAt(CurrentTokens.Count - 1); // Remove the value to the left.
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
							expr2 = Create.CreateMethodCall(fnTokens, method, expr, Script);
						}
					}
					else if (expr.Type is IHasFieldsType) // It's a field access expression.typeof(IHasFieldsType).IsAssignableFrom(expr.Type.GetType())
					{
						var type = (IHasFieldsType)expr.Type;
						if (!type.Fields.ContainsKey(name))
							throw new ApplicationException($"The type {expr.Type.Name} does not have a field named {name}.");
						expr2 = new FieldAccessExpression(expr, name, type.Fields[name]);
						nextIndex++; // Skip over the field name.
					}
					else
						throw new ApplicationException($"The type {expr.Type.Name} does not have a method named {name}.");
					
					var sub = SUB + SubCount++;
					Substitutions.Add(new Identifier(sub), expr2);
					CurrentTokens.Add(new Identifier(sub));
					i = nextIndex -1; // In the next iteration, i == nextIndex.
				}
				#endregion
				else if (Script.CurrentScope.VariableExists(val)) // Variable
				{
					var v = Script.CurrentScope.GetVariable(val);
					IExpression expr;
					if (!v.Mutable && ( v.InitialValue?.IsReifyTimeConst()?? false ) )
						expr = v.InitialValue.Fold();
					else expr = new VariableExpression(val, v.Type);
                    var name = SUB + SubCount++;
					Variables.Add(v);
					Substitutions.Add(new Identifier(name), expr);
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
						int j = i; // Move to the right twice, now looking at token after the opening '('.
						var paramTokens = new List<IToken>();
						/*int lCount = 0;
						int rCount = 0;*/
						int pCount = 0;
						do
						{
							if (++j >= InitialTokens.Count)
								throw new ApplicationException("Mismatched parenthesis...");
							var t = InitialTokens[j];
							var v = t.Value;
							if (v == "(") ++pCount; // ++lCount;
							if (v == ")") --pCount; // ++rCount
							paramTokens.Add(t);
						} while (/*lCount != rCount*/pCount != 0);
						nextIndex = j + 1;
						// Create the function call, add it to the dictionary, and add its identifier to the list (ls).
						var name = SUB + SubCount++;
						var fnCall = Create.CreateFunctionCall(paramTokens, fn, Script);
						Substitutions.Add(new Identifier(name), fnCall);
						CurrentTokens.Add(new Identifier(name));
					}
					i = nextIndex -1; // In the next iteration, i == nextIndex.
				}
				#endregion
				else if(val == "true" || val == "false")
				{
					var name = SUB + SubCount++;
					Substitutions.Add(new Identifier(name), LSN_BoolValue.GetBoolValue(bool.Parse(val)));
					CurrentTokens.Add(new Identifier(name));
				}
				else if(val == "new") // new
				{
					if (i + 1 > InitialTokens.Count)
						throw new ApplicationException("An expresion cannot end with \'new\'.");
					IExpression expr = null;
					int j = i;
					string typeName = InitialTokens[++j].Value; // j points to the type name;
					if (!Script.TypeExists(typeName))
						throw new ApplicationException($"The type \'{typeName}\' could not be found. Are You missing a \'#using\' or \'#include\'?");

					LsnType type = Script.GetType(typeName);
					var structType = type as LsnStructType;
					var recordType = type as RecordType;

					if (structType == null && recordType == null)
						throw new ApplicationException($"Cannot use \'new\' with type \'{typeName}\'.");
					if (j + 2 >= InitialTokens.Count)
						throw new ApplicationException("No parenthesis.");
					var paramTokens = new List<IToken>();
					/*int lCount = 0;
					int rCount = 0;*/
					int pCount = 0;
					do
					{
						if (++j >= InitialTokens.Count)
							throw new ApplicationException("Mismatched parenthesis...");
						var t = InitialTokens[j];
						var v = t.Value;
						if (v == "(") ++pCount; // ++lCount;
						if (v == ")") --pCount; // ++rCount
						paramTokens.Add(t);
					} while (/*lCount != rCount*/pCount != 0);
					var parameters = Create.CreateParamList(paramTokens, -1, Script);
					if (structType != null)
						expr = new StructConstructor(structType, parameters.ToDictionary());
					else // recordType != null
						expr = new RecordConstructor(recordType, parameters.ToDictionary());
					i = j;
					var sub = SUB + SubCount++;
					Substitutions.Add(new Identifier(sub), expr);
					CurrentTokens.Add(new Identifier(sub));
				}
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


		private void ParseInexers()
		{
			var newTokens = new List<IToken>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if (val == "[")
				{
					int nextIndex = i + 1;
					int lCount = 1;
					int rCount = 0;
					int j = 1;
					List<IToken> iTokens = new List<IToken>();
					while (lCount != rCount)
					{
						if (CurrentTokens[i + j].Value == "]")
							rCount++;
						else if (CurrentTokens[i + j].Value == "[")
							lCount++;
						if (lCount == rCount) break;
						iTokens.Add(CurrentTokens[i + j]);
						j++;
					}
					var expr = Build(iTokens, Script, Substitutions.Where(p => iTokens.Contains(p.Key)).ToDictionary(), SubCount);
					var name = SUB + SubCount++;
					var coll = GetExpression(CurrentTokens[CurrentTokens.Count - 1]);
					CurrentTokens.RemoveAt(CurrentTokens.Count - 1);
					if(!(coll.Type is ICollectionType))// typeof(ICollectionType).IsAssignableFrom(coll.Type.GetType()
						throw new ApplicationException($"{coll.Type.Name} cannot be indexed.");
					var t = coll.Type as ICollectionType;
					if (t.IndexType != expr.Type)
						throw new ApplicationException($"{coll.Type.Name} cannot be indexed by type {expr.Type.Name}.");
                    Substitutions.Add(new Identifier(name), new CollectionValueAccessExpression(coll,expr, t.ContentsType));
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
				LsnCore.Operator op = LsnCore.Operator.Add;
				if (val == "*")
					op = LsnCore.Operator.Multiply;
				else if(val == "/")
					op = LsnCore.Operator.Divide;
				else if (val == "%")
					op = LsnCore.Operator.Mod;

				if(op != LsnCore.Operator.Add)
				{
					IExpression left = GetExpression(CurrentTokens[i - 1]);
					IExpression right = GetExpression(CurrentTokens[i + 1]);
					var key = new Tuple<LsnCore.Operator, LsnType>(op, right.Type);

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
					var key = new Tuple<LsnCore.Operator, LsnType>(LsnCore.Operator.Power, right.Type);

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
				LsnCore.Operator op = LsnCore.Operator.Multiply;
				if (val == "+") op = LsnCore.Operator.Add;
				else if (val == "-") op = LsnCore.Operator.Subtract;

				if(op != LsnCore.Operator.Multiply)
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					var key = new Tuple<LsnCore.Operator, LsnType>(op, right.Type);

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
				LsnCore.Operator op = LsnCore.Operator.Multiply;
				if (val == "<") op = LsnCore.Operator.LessThan;
				else if (val == ">") op = LsnCore.Operator.GreaterThan;
				else if (val == "==") op = LsnCore.Operator.Equals;
				else if (val == ">=") op = LsnCore.Operator.GTE;
				else if (val == "<=") op = LsnCore.Operator.LTE;

				if (op != LsnCore.Operator.Multiply)
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					var key = new Tuple<LsnCore.Operator, LsnType>(op, right.Type);

					if (!left.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator {val} is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Operators[key];
					IExpression expr = new BinaryExpression(left, right, opr.Item1, opr.Item2);
					var name = SUB + SubCount++;
					if(newTokens.Count > 0) newTokens.RemoveAt(newTokens.Count - 1);
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
			if (Script.CurrentScope.VariableExists(token.Value))
				return new VariableExpression(token.Value, Script.CurrentScope.GetVariable(token.Value).Type);
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
