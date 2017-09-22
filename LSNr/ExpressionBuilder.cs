using LsnCore;
using LsnCore.Expressions;
using Tokens;
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
		private const string SUB = "Ƨ";

		private readonly Dictionary<Token, IExpression> Substitutions;
		private readonly List<Token> InitialTokens;
		private List<Token> CurrentTokens = new List<Token>();
		private int SubCount = 0;
		private readonly IPreScript Script;

		/// <summary>
		/// The variables used in this expression.
		/// </summary>
		private List<Variable> Variables = new List<Variable>();

		private ExpressionBuilder(List<Token> tokens, IPreScript script)
		{
			InitialTokens = tokens;
			Script = script;
			Substitutions = new Dictionary<Token, IExpression>();
		}


		private ExpressionBuilder(List<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> subs, int count)
		{
			InitialTokens = tokens;
			Script = script;
			Substitutions = subs.ToDictionary();
			SubCount = count;
			
		}

		private IExpression Parse()
		{
			if(InitialTokens.Count == 1)
			{
				if (Substitutions.ContainsKey(InitialTokens[0])) return Substitutions[InitialTokens[0]];
				return Create.SingleTokenExpress(InitialTokens[0], Script, null, Variables);
			}
			//CurrentTokens = InitialTokens.ToList();
			//if(CurrentTokens.Count != 1)
			//	ParseParenthesis();
			ParseVariablesAndFunctions();
			ParseInexers();// Put this with functions and variables?
			ParseMemberAccess();
			if (CurrentTokens.Count != 1)
				ParseParenthesis();
			if (CurrentTokens.Any(t => t.Value == "^")) ParseExponents();
			if (CurrentTokens.Any(t => { var v = t.Value; return v == "*" || v == "/" || v == "%"; }))
				ParseMultDivMod();
			if (CurrentTokens.Any(t => { var v = t.Value; return v == "+" || v == "-"; }))
				ParseAddSubtract();
			ParseComparisons();
			if(CurrentTokens.Count != 1) { throw new ApplicationException("This should not happen."); }
			var expr = Substitutions[CurrentTokens[0]].Fold();
			if(! expr.IsReifyTimeConst())
				foreach (var v in Variables) v.AddUser(expr as IExpressionContainer);
			return expr;
		}

		/// <summary>
		/// Parse variables, boolean and null/nil/none/nothing literals, functions, methods, and fields.
		/// </summary>
		private void ParseVariablesAndFunctions()
		{
			string name; IExpression expr;
			for (int i = 0; i < InitialTokens.Count; i++)
			{
				var val = InitialTokens[i].Value;
				switch (Script.CheckSymbol(val))
				{
					case SymbolType.Undefined:
					{
						if (val == "true" || val == "false")
						{
							name = SUB + SubCount++;
							Substitutions.Add(new Token(name,-1,TokenType.Substitution), LsnBoolValue.GetBoolValue(bool.Parse(val)));
							CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
						}
						else if (val == "new") // new
						{
							#region new
							if (i + 1 > InitialTokens.Count)
								throw new ApplicationException("An expression cannot end with \'new\'.");
							expr = null;
							int j = i;
							//string typeName = InitialTokens[++j].Value; // j points to the type name;
							var typeId = Script.ParseTypeId(InitialTokens, i + 1, out j);
							// j points to the thing after the end of the name.
							if (j < 0)
								throw new ApplicationException($"Error line {InitialTokens[i].LineNumber}: Failed to parse type name...");
							j--; // j points to the last token of the name.
								 //if (!Script.TypeExists(typeName))
								 //	throw new ApplicationException($"The type \'{typeName}\' could not be found. Are You missing a \'#using\' or \'#include\'?");						

							LsnType type = typeId.Type;//Script.GetType(typeName);
							var structType = type as RecordType;
							var recordType = type as StructType;
							var listType = type as LsnListType;

							if (structType == null && recordType == null && listType == null)
								throw new ApplicationException($"Cannot use \'new\' with type \'{typeId.Name}\'.");
							if (j + 2 >= InitialTokens.Count)
								throw new ApplicationException("No parenthesis.");
							if (listType == null)
							{
								var paramTokens = new List<Token>();
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
								var parameters = Create.CreateParamList(paramTokens, -1, Script, Substitutions.Where(s => paramTokens.Contains(s.Key)).ToDictionary());
								if (structType != null)
									expr = new StructConstructor(structType, parameters.ToDictionary());
								else // recordType != null
									expr = new RecordConstructor(recordType, parameters.ToDictionary());
							}
							else
							{
								if (!(j + 2 <= InitialTokens.Count && InitialTokens[j + 1].Value == "(" && InitialTokens[j + 2].Value == ")"))
									throw new ApplicationException("No parenthesis...");
								expr = new ListConstructor(listType);
								j += 2;
							}
							i = j;
							name = SUB + SubCount++;
							Substitutions.Add(new Token(name, -1, TokenType.Substitution), expr);
							CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
							#endregion
						}
						else if (val == "this" )
						{
							var preScrFn = Script as PreScriptObjectFunction;
							if (preScrFn == null)
								throw new ApplicationException("...");
							name = SUB + SubCount++;
							Substitutions.Add(new Token(name, -1, TokenType.Substitution), new VariableExpression(0, preScrFn.Parent.Id));
							CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
						}
						else if (val == "host")
						{
							var preScrFn = Script as PreScriptObjectFunction;
							if (preScrFn == null)
								throw new ApplicationException("...");
							name = SUB + SubCount++;
							Substitutions.Add(new Token(name, -1, TokenType.Substitution), new HostInterfaceAccessExpression(new VariableExpression(0, preScrFn.Parent.Id), preScrFn.Parent.HostType.Id));
							CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
						}
						else
							CurrentTokens.Add(InitialTokens[i]);
						break;
					}
					case SymbolType.Variable:
					{
						var v = Script.CurrentScope.GetVariable(val);
						expr = v.GetAccessExpression();
						/*if (!v.Mutable && ( v.InitialValue?.IsReifyTimeConst()?? false ) )
							expr = v.InitialValue.Fold();
						else expr = new VariableExpression(val, v.Type);*/
						name = SUB + SubCount++;
						Variables.Add(v); //TODO: Create a method for adding a substitution.
						Substitutions.Add(new Token(name, -1, TokenType.Substitution), expr);
						CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
						break;
					}
					case SymbolType.UniqueScriptObject:
						{
							expr = new UniqueScriptObjectAccessExpression(val, Script.GetTypeId(val));
							name = SUB + SubCount++;
							Substitutions.Add(new Token(name, -1, TokenType.Substitution), expr);
							CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
							break;
						}
					case SymbolType.GlobalVariable:
						throw new NotImplementedException();
					case SymbolType.Field:
						{
							var preScrFn = Script as PreScriptObjectFunction;
							var preScr = preScrFn.Parent;
							IExpression scrObjExpr = new VariableExpression(0, preScr.Id);
							var f = preScr.GetField(val);
							expr = new FieldAccessExpression(scrObjExpr, f);
							name = SUB + SubCount++;
							Substitutions.Add(new Token(name, -1, TokenType.Substitution), expr);
							CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
						}
						break;
					case SymbolType.Property:
					{
						var preScrFn = Script as PreScriptObjectFunction;
						var preScr = preScrFn.Parent;
						expr = new PropertyAccessExpression(new VariableExpression(0, preScr.Id), preScr.GetPropertyIndex(val), preScr.GetProperty(val).Type);
						name = SUB + SubCount++;
						Substitutions.Add(new Token(name, -1, TokenType.Substitution), expr);
						CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
						break;
					}
					case SymbolType.Function:
					{
						#region Function
							var fn = Script.GetFunction(val);
							int nextIndex = i + 1; // This is the default next index.
							if (InitialTokens[i + 1].Value != "(")
							{
								if (fn.Parameters.Count != 0)
									throw new ApplicationException(); // Or throw or log something...
								var fnCall = fn.CreateCall(new List<Tuple<string, IExpression>>());
								name = SUB + SubCount++;
								Substitutions.Add(new Token(name, -1, TokenType.Substitution), fnCall);
								CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
							}
							else
							{
								//int lCount = 1;
								//int rCount = 0;
								int j = i; // Move to the right twice, now looking at token after the opening '('.
								var paramTokens = new List<Token>();
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
								name = SUB + SubCount++;
								var pt = paramTokens.Skip(1).Take(paramTokens.Count - 2).ToList();
								expr = Create.CreateFunctionCall(pt, fn, Script, Substitutions.Where(p => pt.Contains(p.Key)).ToDictionary());
								Substitutions.Add(new Token(name, -1, TokenType.Substitution), expr);
								CurrentTokens.Add(new Token(name, -1, TokenType.Substitution));
							}
							i = nextIndex - 1; // In the next iteration, i == nextIndex.
							#endregion
						break;
					}
					case SymbolType.ScriptObjectMethod:
					{
						var preScrFn = Script as PreScriptObjectFunction;
						IExpression scrObjExpr = new VariableExpression(0, preScrFn.Parent.Id);
						throw new NotImplementedException();
					}
					case SymbolType.HostInterfaceMethod:
					{
							/*var preScrFn = Script as PreScriptObjectFunction;
							IExpression scrObjExpr = new VariableExpression(0, preScrFn.Parent.Id);
							throw new NotImplementedException();*/
							CurrentTokens.Add(InitialTokens[i]);
							break;
					}
					default:
						break;
				}

			}
		}

		private void ParseMemberAccess()
		{
			var newTokens = new List<Token>(); int lCount ,rCount,j;
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if (val == ".") //Member Access
				{
					#region .
					if (i == 0)
						throw new ApplicationException("An expression cannot start with \'.\'.");
					if (i + 1 > CurrentTokens.Count)
						throw new ApplicationException("An expression cannot end with \'.\'.");
					IExpression leftExpr;

					int nextIndex = i + 1; // This is the default next index. It now points to the thing after '.', i.e. the member name
										   // Get the expression for the object calling the method or accessing a member
					if (CurrentTokens[i - 1].Value == ")")
					{
						// I'm not too sure this will work...
						var tokens = new List<Token>();
						rCount = 1;
						lCount = 0;
						j = i - 1;
						while (true) //TODO: Fix this!!!
						{
							if (j < 0) throw new ApplicationException("Mismatched parenthesis.");
							var v = CurrentTokens[j].Value;
							newTokens.RemoveAt(newTokens.Count - 1); // Remove the token at the end of the list.
							if (v == ")") rCount++;
							else if (v == "(") lCount++;
							if (lCount == rCount) break;
							tokens.Add(CurrentTokens[j]);
							j--;
						}
						leftExpr = Build(tokens, Script);
					}
					else
					{
						leftExpr = GetExpression(CurrentTokens[i - 1]);
						newTokens.RemoveAt(newTokens.Count - 1); // Remove the value to the left.
					}

					var memberName = CurrentTokens[i + 1].Value;
					IExpression memberExpression = null;
					// Is it a method call or a field access expression?
					if (leftExpr.Type.Type != null && leftExpr.Type.Type.Methods.ContainsKey(memberName)) // It's a method call.
					{
						var method = leftExpr.Type.Type.Methods[memberName];
						if (method.Parameters.Count == 1) // The only argument is the object on which it is called
						{
							if (!(i + 3 < CurrentTokens.Count && CurrentTokens[i + 2].Value == "(" && CurrentTokens[i + 3].Value == ")"))
								throw new ApplicationException($"Error line {CurrentTokens[i + 1].LineNumber}: Improperly formated method call.");
							memberExpression = method.CreateMethodCall
								  (new List<Tuple<string, IExpression>>(), leftExpr, true/*Script.MethodIsIncluded(name)*/);
							nextIndex = i + 4; //Skip the name and the parenthesis. It now points to the thing after the closing ')'.
						}
						else
						{
							lCount = 1;
							rCount = 0;
							j = 3; // Move to the right twice, now looking at token after the opening '('.
							var fnTokens = new List<Token>();
							while (lCount != rCount)
							{
								if (CurrentTokens[i + j].Value == ")")
								{
									rCount++;
									if (lCount == rCount) break;
								}
								else if (CurrentTokens[i + j].Value == "(")
								{
									lCount++;
								}
								fnTokens.Add(CurrentTokens[i + j]);
								j++;
							}
							memberExpression = Create.CreateMethodCall(fnTokens, method, leftExpr, Script, Substitutions.Where(s => fnTokens.Contains(s.Key)).ToDictionary());
							nextIndex += j; // nextIndex = i + 1 + j. Points to the thing after the closing ')'.
						}
					}
					
					else if (leftExpr.Type.Type is HostInterfaceType)
					{
						var type = leftExpr.Type.Type as HostInterfaceType;
						if (type.HasMethod(memberName))
						{
							var methodDef = type.GetMethodDefinition(memberName);
							IExpression[] args;
							if (methodDef.Parameters.Count == 0)
							{ // Note: 'i' still points to '.'
								if (!(i + 3 < CurrentTokens.Count && CurrentTokens[i + 2].Value == "(" && CurrentTokens[i + 3].Value == ")"))
									throw new ApplicationException($"Error line {CurrentTokens[i + 1].LineNumber}: Improperly formated method call.");
								args = new IExpression[0];
								nextIndex = i + 4; //Skip the name and the parenthesis. It now points to the thing after the closing ')'.
							}
							else
							{
								lCount = 1;
								rCount = 0;
								j = 3; // Move to the right twice, now looking at token after the opening '('.
								var fnTokens = new List<Token>();
								while (lCount != rCount)
								{
									if (CurrentTokens[i + j].Value == ")")
									{
										rCount++;
										if (lCount == rCount) break;
									}
									else if (CurrentTokens[i + j].Value == "(")
									{
										lCount++;
									}
									fnTokens.Add(CurrentTokens[i + j]);
									j++;
								}
								nextIndex += j; // nextIndex = i + 1 + j. Points to the thing after the closing ')'.
								args = methodDef.CreateArgsArray(
									Create.CreateParamList(fnTokens, methodDef.Parameters.Count, Script, Substitutions.Where(p => fnTokens.Contains(p.Key)).ToDictionary()));
							}
							memberExpression = new HostIntefaceMethodCall(methodDef, leftExpr, args);
						}
						else
							throw new ApplicationException($"Error line {CurrentTokens[i].LineNumber}: The HostInterface type '{type.Name}' does not have a method '{memberName}'.");
					}
					else if (leftExpr.Type.Type is ScriptObjectType)
					{
						var scrObjType = leftExpr.Type.Type as ScriptObjectType;
						var method = scrObjType.GetMethod(memberName);
						if (method.Parameters.Count == 1) // The only argument is the object on which it is called
						{
							if (!(i + 3 < CurrentTokens.Count && CurrentTokens[i + 2].Value == "(" && CurrentTokens[i + 3].Value == ")"))
								throw new ApplicationException($"Error line {CurrentTokens[i + 1].LineNumber}: Improperly formated method call.");
							memberExpression = method.CreateMethodCall
								  (new List<Tuple<string, IExpression>>(), leftExpr, true/*Script.MethodIsIncluded(name)*/);
							nextIndex = i + 4; //Skip the name and the parenthesis. It now points to the thing after the closing ')'.
						}
						else
						{
							lCount = 1;
							rCount = 0;
							j = 3; // Move to the right twice, now looking at token after the opening '('.
							var fnTokens = new List<Token>();
							while (lCount != rCount)
							{
								if (CurrentTokens[i + j].Value == ")")
								{
									rCount++;
									if (lCount == rCount) break;
								}
								else if (CurrentTokens[i + j].Value == "(")
								{
									lCount++;
								}
								fnTokens.Add(CurrentTokens[i + j]);
								j++;
							}
							memberExpression = Create.CreateMethodCall(fnTokens, method, leftExpr, Script, Substitutions.Where(s => fnTokens.Contains(s.Key)).ToDictionary());
							nextIndex += j; // nextIndex = i + 1 + j. Points to the thing after the closing ')'.

						}
					}
					else if (leftExpr.Type.Type is IHasFieldsType) // It's a field access expression.
																   //typeof(IHasFieldsType).IsAssignableFrom(expr.Type.GetType())
					{
						var type = (IHasFieldsType)leftExpr.Type.Type;
						var field = type.FieldsB.FirstOrDefault(f => f.Name == memberName);
						if (field.Name == null)
							throw new ApplicationException($"The type {leftExpr.Type.Name} does not have a field named {memberName}.");
						memberExpression = new FieldAccessExpression(leftExpr, memberName, field.Type);
						nextIndex++; // Skip over the field name.
					}
					else
						throw new ApplicationException($"The type {leftExpr.Type.Name} does not have a method named {memberName}."); 
					// Two unique script objects calling methods on each other. ... Only one of them is 'complete'.

					var sub = SUB + SubCount++;
					Substitutions.Add(new Token(sub, -1, TokenType.Substitution), memberExpression);
					newTokens.Add(new Token(sub, -1, TokenType.Substitution));
					i = nextIndex - 1; // In the next iteration, i == nextIndex.
					#endregion
				}
				else
					newTokens.Add(CurrentTokens[i]);
			}
			CurrentTokens = newTokens;
		}

		private void ParseParenthesis()
		{
			var newTokens = new List<Token>();
			for(int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if(val == "(")
				{
					int nextIndex = i + 1;
					int lCount = 1;
					int rCount = 0;
					int j = 1;
					List<Token> pTokens = new List<Token>();
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
					Substitutions.Add(new Token(name, -1, TokenType.Substitution), expr);
					newTokens.Add(new Token(name, -1, TokenType.Substitution));
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
			var newTokens = new List<Token>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if (val == "[")
				{
					int nextIndex = i + 1;
					int lCount = 1;
					int rCount = 0;
					int j = 1;
					List<Token> iTokens = new List<Token>();
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
					var coll = GetExpression(newTokens[newTokens.Count - 1]);
					newTokens.RemoveAt(newTokens.Count - 1);
					var t = coll.Type.Type as ICollectionType;
					if (t == null)// typeof(ICollectionType).IsAssignableFrom(coll.Type.GetType()
						throw new ApplicationException($"{coll.Type.Name} cannot be indexed.");
					if (t.IndexType != expr.Type)
						throw new ApplicationException($"{coll.Type.Name} cannot be indexed by type {expr.Type.Name}.");
					var nt = new Token(name, -1, TokenType.Substitution);
					Substitutions.Add(nt, new CollectionValueAccessExpression(coll,expr, t.ContentsType.Id));
					newTokens.Add(nt);
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
			var newTokens = new List<Token>();
			for(int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if (val.Length > 2)
				{
					newTokens.Add(CurrentTokens[i]);
					continue;
				}
				var op = BinaryOperation.Sum;
				if (val == "*")
					op = BinaryOperation.Product;
				else if(val == "/")
					op = BinaryOperation.Quotient;
				else if (val == "%")
					op = BinaryOperation.Modulus;

				if(op != BinaryOperation.Sum)
				{
					IExpression left = GetExpression(CurrentTokens[i - 1]);
					IExpression right = GetExpression(CurrentTokens[i + 1]);
					/*var key = new Tuple<LsnCore.Operator, TypeId>(op, right.Type);

					if (!left.Type.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator {val} is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Type.Operators[key];*/
					var argType = BinaryExpression.GetArgTypes(left.Type, right.Type);
					IExpression expr = new BinaryExpression(left, right, op, argType); //new BinaryExpression(left, right, opr.Item1, opr.Item2, op);
					var name = SUB + SubCount++;
					newTokens.RemoveAt(newTokens.Count - 1);
					var nt = new Token(name, -1, TokenType.Substitution);
					Substitutions.Add(nt, expr);
					newTokens.Add(nt);
					i++; // This skips to the token after the right hand side of this expression.
				}
				else newTokens.Add(CurrentTokens[i]);
				
			}
			CurrentTokens = newTokens;
		}


		private void ParseExponents()
		{
			var newTokens = new List<Token>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				if (CurrentTokens[i].Value == "^")
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					/*var key = new Tuple<LsnCore.Operator, TypeId>(LsnCore.Operator.Power, right.Type);

					if (!left.Type.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator ^ is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Type.Operators[key];*/
					var argTypes = BinaryExpression.GetArgTypes(left.Type, right.Type);
					IExpression expr = new BinaryExpression(left, right, BinaryOperation.Power, argTypes); //new BinaryExpression(left, right, opr.Item1, opr.Item2, LsnCore.Operator.Power);
					var name = SUB + SubCount++;
					newTokens.RemoveAt(newTokens.Count - 1);
					var nt = new Token(name, -1, TokenType.Substitution);
					Substitutions.Add(nt, expr);
					newTokens.Add(nt);
					i++; // This skips to the token after the right hand side of this expression.
				}
				else newTokens.Add(CurrentTokens[i]);
			}
			CurrentTokens = newTokens;
		}


		private void ParseAddSubtract()
		{
			var newTokens = new List<Token>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				var op = BinaryOperation.Product;
				if (val == "+") op = BinaryOperation.Sum;
				else if (val == "-") op = BinaryOperation.Difference;

				if(op != BinaryOperation.Product)
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					/*var key = new Tuple<LsnCore.Operator, TypeId>(op, right.Type);

					if (!left.Type.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator {val} is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Type.Operators[key];*/
					var argTypes = BinaryExpression.GetArgTypes(left.Type, right.Type);
					IExpression expr = new BinaryExpression(left, right, op, argTypes);//new BinaryExpression(left, right, opr.Item1, opr.Item2, op);
					var name = SUB + SubCount++;
					newTokens.RemoveAt(newTokens.Count - 1);
					var nt = new Token(name, -1, TokenType.Substitution);
					Substitutions.Add(nt, expr);
					newTokens.Add(nt);
					i++; // This skips to the token after the right hand side of this expression.
				}
				else newTokens.Add(CurrentTokens[i]);
			}
			CurrentTokens = newTokens;
		}


		private void ParseComparisons()
		{
			var newTokens = new List<Token>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if (val.Length > 2) continue;
				var op = BinaryOperation.Product;
				switch (val)
				{
					case "<":
						op = BinaryOperation.LessThan;
						break;
					case ">":
						op = BinaryOperation.GreaterThan;
						break;
					case "==":
						op = BinaryOperation.Equal;
						break;
					case "!=":
						op = BinaryOperation.NotEqual;
						break;
					case ">=":
						op = BinaryOperation.GreaterThanOrEqual;
						break;
					case "<=":
						op = BinaryOperation.LessThanOrEqual;
						break;
				}

				if (op != BinaryOperation.Product)
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					/*var key = new Tuple<Operator, TypeId>(op,right.Type);


					if (!left.Type.Type.Operators.ContainsKey(key))
						throw new ApplicationException(
							$"The operator {val} is not defined for type {left.Type.Name} and {right.Type.Name}.");
					var opr = left.Type.Type.Operators[key];*/
					var argTypes = BinaryExpression.GetArgTypes(left.Type, right.Type);
					IExpression expr = new BinaryExpression(left, right, op, argTypes);//new BinaryExpression(left, right, opr.Item1, opr.Item2, op);
					var name = SUB + SubCount++;
					if(newTokens.Count > 0) newTokens.RemoveAt(newTokens.Count - 1);
					var nt = new Token(name, -1, TokenType.Substitution);
					Substitutions.Add(nt, expr);
					newTokens.Add(nt);
					i++; // This skips to the token after the right hand side of this expression.
				}
				else newTokens.Add(CurrentTokens[i]);
			}
			CurrentTokens = newTokens;
		}


		private IExpression GetExpression(Token token)
		{
			if (Substitutions.ContainsKey(token)) return Substitutions[token];
			return Create.SingleTokenExpress(token, Script);
			/*if (Script.CurrentScope.VariableExists(token.Value))
				return new VariableExpression(token.Value, Script.CurrentScope.GetVariable(token.Value).Type);
			var type = token.GetType();
            if (type == typeof(FloatToken)) // It's a double literal.
				return new DoubleValue(((FloatToken)token).DVal);
			if (type == typeof(IntToken)) // It's an integer literal.
				return new IntValue(((IntToken)token).IVal);
			if (type == typeof(StringToken)) // It's a string literal.
				return new StringValue(token.Value);
			return null;*/
		}
		

		public static IExpression Build(List<Token> tokens, IPreScript script)
		{
			var b = new ExpressionBuilder(tokens, script);
			return b.Parse();
		}


		public static IExpression Build(List<Token> tokens, IPreScript script, IReadOnlyDictionary<Token,IExpression> subs, int i)
		{
			var b = new ExpressionBuilder(tokens, script, subs, i);
			return b.Parse();
		}

	}
}
