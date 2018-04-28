using LsnCore;
using LsnCore.Expressions;
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
			if (InitialTokens.Count == 1)
			{
				if (Substitutions.ContainsKey(InitialTokens[0])) return Substitutions[InitialTokens[0]];
				return Create.SingleTokenExpress(InitialTokens[0], Script, null, Variables);
			}
			CurrentTokens = InitialTokens.ToList();
			if (InitialTokens.Count != 1)
				ParseParenthesis2();
			ParseVariablesAndFunctions();
			ParseIndexers();// Put this with functions and variables?
			ParseMemberAccess();
			/*if (CurrentTokens.Count != 1)
				ParseParenthesis();*/
			if (CurrentTokens.Any(t => t.Value == "^")) ParseExponents();
			if (CurrentTokens.Any(t => { var v = t.Value; return v == "*" || v == "/" || v == "%"; }))
				ParseMultDivMod();
			if (CurrentTokens.Any(t => { var v = t.Value; return v == "+" || v == "-"; }))
				ParseAddSubtract();
			ParseComparisons();
			if (CurrentTokens.Count != 1)
				throw new ApplicationException("This should not happen.");
			var expr = Substitutions[CurrentTokens[0]].Fold();
			if (!expr.IsReifyTimeConst())
				foreach (var v in Variables) v.AddUser(expr as IExpressionContainer);
			return expr;
		}

		/// <summary>
		/// Parse variables, boolean and null/nil/none/nothing literals, functions, methods, and fields.
		/// </summary>
		private void ParseVariablesAndFunctions()
		{
			var newTokens = new List<Token>();
			string name; IExpression expr;
			void sub(IExpression ex)
			{
				name = SUB + SubCount++;
				Substitutions.Add(new Token(name, -1, TokenType.Substitution), ex);
				newTokens.Add(new Token(name, -1, TokenType.Substitution));
			}
			List<Token> par(int i, ref int nextIndex)
			{
				int j = i;
				var paramTokens = new List<Token>();
				int pCount = 0;
				do
				{
					if (++j >= CurrentTokens.Count)
						throw new LsnrParsingException(CurrentTokens[i], "Mismatched parenthesis.", Script.Path);
					var t = CurrentTokens[j];
					var v = t.Value;
					if (v == "(") ++pCount;
					if (v == ")") --pCount;
					paramTokens.Add(t);
				} while (pCount != 0);
				nextIndex = j + 1;
				// Create the function call, add it to the dictionary, and add its identifier to the list (ls).
				return paramTokens.Skip(1).Take(paramTokens.Count - 2).ToList();
			}
			IList<Tuple<string,IExpression>> paren(int i, int paramCount, ref int nextIndex)
			{
				var pt = par(i, ref nextIndex);
				return Create.CreateParamList(pt, paramCount, Script, Substitutions.Where(p => pt.Contains(p.Key)).ToDictionary());
			}

			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				switch (Script.CheckSymbol(val))
				{
					case SymbolType.Undefined:
						{
							if (val == "true" || val == "false")
								sub(LsnBoolValue.GetBoolValue(bool.Parse(val)));
							else if (val == "new") // new
							{
								#region new
								if (i + 1 > CurrentTokens.Count)
									throw new LsnrParsingException(CurrentTokens[i], "An expression cannot end with \'new\'.", Script.Path);
								expr = null;
								int j = i;
								//string typeName = CurrentTokens[++j].Value; // j points to the type name;
								var typeId = Script.ParseTypeId(CurrentTokens, i + 1, out j);
								// j points to the thing after the end of the name.
								if (j < 0)
									throw new LsnrParsingException(CurrentTokens[i], "Failed to parse type name.", Script.Path);
								j--; // j points to the last token of the name.
								var type = typeId.Type;//Script.GetType(typeName);
								var recordType = type as RecordType;
								var structType = type as StructType;
								var listType = type as LsnListType;

								if (recordType == null && structType == null && listType == null)
									throw new LsnrParsingException(CurrentTokens[i], $"Cannot use \'new\' with type \'{typeId.Name}\'.", Script.Path);
								if (j + 2 >= CurrentTokens.Count)
									throw new LsnrParsingException(CurrentTokens[i], "No parenthesis.", Script.Path);
								if (listType == null)
								{
									var paramTokens = new List<Token>();
									int pCount = 0;
									do
									{
										if (++j >= CurrentTokens.Count)
											throw new LsnrParsingException(CurrentTokens[j - 1], "Mismatched parenthesis.", Script.Path);
										var t = CurrentTokens[j];
										var v = t.Value;
										if (v == "(") ++pCount; // ++lCount;
										if (v == ")") --pCount; // ++rCount
										paramTokens.Add(t);
									} while (/*lCount != rCount*/pCount != 0);
									var parameters = Create.CreateParamList(paramTokens, -1, Script, Substitutions.Where(s => paramTokens.Contains(s.Key)).ToDictionary());
									if (recordType != null)
										expr = new RecordConstructor(recordType, parameters.ToDictionary());
									else // recordType != null
										expr = new StructConstructor(structType, parameters.ToDictionary());
								}
								else
								{
									if (!(j + 2 <= CurrentTokens.Count && CurrentTokens[j + 1].Value == "(" && CurrentTokens[j + 2].Value == ")"))
										throw new LsnrParsingException(CurrentTokens[j], "No parenthesis.", Script.Path);
									expr = new ListConstructor(listType);
									j += 2;
								}
								i = j;
								sub(expr);
								#endregion
							}
							else if (val == "this")
							{
								var preScrFn = Script as PreScriptClassFunction;
								if (preScrFn == null)
									throw new LsnrParsingException(CurrentTokens[i], "Cannot use 'this' outside of a script object", Script.Path); ;
								sub(new VariableExpression(0, preScrFn.Parent.Id));
							}
							else if (val == "host")
							{
								var preScrFn = Script as PreScriptClassFunction;
								if (preScrFn == null)
									throw new LsnrParsingException(CurrentTokens[i], "Cannot use 'host' outside of a script object", Script.Path);
								sub(new HostInterfaceAccessExpression(preScrFn.Parent.HostType.Id));
							}
							else
								newTokens.Add(CurrentTokens[i]);
							break;
						}
					case SymbolType.Variable:
						{
							var v = Script.CurrentScope.GetVariable(val);
							Variables.Add(v); //TODO: Create a method for adding a substitution.
							sub(v.AccessExpression);
							break;
						}
					case SymbolType.UniqueScriptObject:
						sub(new UniqueScriptObjectAccessExpression(val, Script.GetTypeId(val)));
						break;
					case SymbolType.GlobalVariable:
						throw new NotImplementedException();
					case SymbolType.Field:
						{
							var preScrFn = Script as PreScriptClassFunction;
							var preScr = preScrFn.Parent;
							IExpression scrObjExpr = new VariableExpression(0, preScr.Id);
							var f = preScr.GetField(val);
							sub(new FieldAccessExpression(scrObjExpr, f));
						}
						break;
					case SymbolType.Property:
						{
							// ToDo: what about accessing the properties of another object?
							var preScrFn = Script as PreScriptClassFunction;
							var preScr = preScrFn.Parent;
							sub(new PropertyAccessExpression(new VariableExpression(0, preScr.Id), preScr.GetPropertyIndex(val), preScr.GetProperty(val).Type));
							break;
						}
					case SymbolType.Function:
						{
							#region Function
							var fn = Script.GetFunction(val);
							int nextIndex = i + 1; // This is the default next index.
							if (i + 1 >= CurrentTokens.Count || CurrentTokens[i + 1].Value != "(")
							{
								if (fn.Parameters.Count != 0)
									throw new LsnrParsingException(CurrentTokens[i], $"Cannot call the function '{val}' without argument(s).", Script.Path); // Or throw or log something...
								sub(fn.CreateCall(new List<Tuple<string, IExpression>>()));
							}
							else
							{
								var pt = par(i, ref nextIndex);
								sub(Create.CreateFunctionCall(pt, fn, Script, Substitutions.Where(p => pt.Contains(p.Key)).ToDictionary()));
							}
							i = nextIndex - 1; // In the next iteration, i == nextIndex.
							#endregion
							break;
						}
					case SymbolType.ScriptClassMethod:
						{
							if (i > 0 && CurrentTokens[i - 1].Value == ".") // It's being called on an object, could be something other than
																			// this.
							{
								newTokens.Add(CurrentTokens[i]);
								break;
							}
							var preScrFn = Script as PreScriptClassFunction;
							IExpression scrObjExpr = new VariableExpression(0, preScrFn.Parent.Id);
							var meth = preScrFn.Parent.GetMethodSignature(val);
							int nextIndex = i + 1; // This is the default next index.
							if (i + 1 >= CurrentTokens.Count || CurrentTokens[i + 1].Value != "(")
							{
								if (meth.Parameters.Count != 1)
									throw new LsnrParsingException(CurrentTokens[i], $"Cannot call the function '{val}' without argument(s).", Script.Path); // Or throw or log something...
								sub(new ScriptObjectVirtualMethodCall(new IExpression[] { scrObjExpr }, val));
							}
							else
							{
								var args = paren(i, meth.Parameters.Count - 1, ref nextIndex);
								args.Add(new Tuple<string, IExpression>("self", scrObjExpr));
								sub(new ScriptObjectVirtualMethodCall(meth.CreateArgsArray(args), val));
							}
							i = nextIndex - 1; // In the next iteration, i == nextIndex.
							break;
						}
					case SymbolType.HostInterfaceMethod:
						{
							/*throw new NotImplementedException();*/
							if (i >=2 && CurrentTokens[i - 2].Value == "host")
							{
								newTokens.Add(CurrentTokens[i]);
								break;
							}
							var preScrFn = Script as PreScriptClassFunction;
							var meth = preScrFn.Parent.GetHostMethodSignature(val);
							int nextIndex = i + 1; // This is the default next index.
							if (i + 1 >= CurrentTokens.Count || CurrentTokens[i + 1].Value != "(")
							{
								if (meth.Parameters.Count != 1)
									throw new LsnrParsingException(CurrentTokens[i], $"Cannot call the function '{val}' without argument(s).", Script.Path); // Or throw or log something...
								sub(new HostInterfaceMethodCall(meth, new HostInterfaceAccessExpression(), new IExpression[0]));
							}
							else if (meth.Parameters.Count == 0)
							{
								if (CurrentTokens[i + 1].Value != "(")
									throw new ApplicationException();
								if (i + 2 >= CurrentTokens.Count)
									throw new ApplicationException();
								if (CurrentTokens[i + 2].Value != ")")
									throw new ApplicationException();
								sub(new HostInterfaceMethodCall(meth, new HostInterfaceAccessExpression(), meth.CreateArgsArray(new List<Tuple<string, IExpression>>())));
							}
							else
							{

								var args = paren(i, meth.Parameters.Count - 1, ref nextIndex);
								sub(new HostInterfaceMethodCall(meth, new HostInterfaceAccessExpression(), meth.CreateArgsArray(args)));
							}
							i = nextIndex - 1; // In the next iteration, i == nextIndex.
							break;
						}
					default:
						newTokens.Add(CurrentTokens[i]);
						break;
				}
			}
			CurrentTokens = newTokens;
		}

		private IExpression _ParseMethod(string memberName, IExpression leftExpr, int i, ref int nextIndex)
		{
			var method = leftExpr.Type.Type.Methods[memberName];
			if (method.Parameters.Count == 1) // The only argument is the object on which it is called
			{
				if (!(i + 3 < CurrentTokens.Count && CurrentTokens[i + 2].Value == "(" && CurrentTokens[i + 3].Value == ")"))
					throw new LsnrParsingException(CurrentTokens[i + 1], $"Improperly formatted method call.", Script.Path);
				nextIndex = i + 4; //Skip the name and the parenthesis. It now points to the thing after the closing ')'.
				return method.CreateMethodCall(new List<Tuple<string, IExpression>>(), leftExpr);
			}
			int lCount = 1;
			int rCount = 0;
			int j = 3; // Move to the right twice, now looking at token after the opening '('.
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
			return Create.CreateMethodCall(fnTokens, method, leftExpr, Script, Substitutions.Where(s => fnTokens.Contains(s.Key)).ToDictionary());
		}

		private IExpression ParseHostInterfaceMethodCall(HostInterfaceType type, IExpression leftExpr, string memberName, int i, ref int nextIndex)
		{
			var methodDef = type.GetMethodDefinition(memberName);
			IExpression[] args;
			if (methodDef.Parameters.Count == 0)
			{ // Note: 'i' still points to '.'
				if (!(i + 3 < CurrentTokens.Count && CurrentTokens[i + 2].Value == "(" && CurrentTokens[i + 3].Value == ")"))
					throw new LsnrParsingException(CurrentTokens[i], "Improperly formatted method call.", Script.Path);
				args = new IExpression[0];
				nextIndex = i + 4; //Skip the name and the parenthesis. It now points to the thing after the closing ')'.
			}
			else
			{
				var lCount = 1;
				var rCount = 0;
				var j = 3; // Move to the right twice, now looking at token after the opening '('.
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
			return new HostInterfaceMethodCall(methodDef, leftExpr, args);
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
						throw new LsnrParsingException(CurrentTokens[i], "An expression cannot start with '.'.", Script.Path);
					if (i + 1 > CurrentTokens.Count)
						throw new LsnrParsingException(CurrentTokens[i], "An expression cannot end with '.'.", Script.Path);
					IExpression leftExpr;

					var nextIndex = i + 1; // This is the default next index. It now points to the thing after '.', i.e. the member name
										   // Get the expression for the object calling the method or accessing a member
					leftExpr = GetExpression(CurrentTokens[i - 1]);
					newTokens.RemoveAt(newTokens.Count - 1); // Remove the value to the left.

					var memberName = CurrentTokens[i + 1].Value;
					IExpression memberExpression = null;
					// Is it a method call or a field access expression?
					if (leftExpr.Type.Type != null && leftExpr.Type.Type.Methods.ContainsKey(memberName)) // It's a method call.
					{
						memberExpression = _ParseMethod(memberName, leftExpr, i, ref nextIndex);
					}
					else if (leftExpr.Type.Type is HostInterfaceType)
					{
						var type = leftExpr.Type.Type as HostInterfaceType;
						if (type.HasMethod(memberName))
						{
							memberExpression = ParseHostInterfaceMethodCall(type, leftExpr, memberName, i, ref nextIndex);
						}
						else
							throw new LsnrParsingException(CurrentTokens[i], $"The HostInterface type '{type.Name}' does not have a method '{memberName}'.", Script.Path);
					}
					else if (leftExpr.Type.Type is ScriptClass)
					{
						var scrObjType = leftExpr.Type.Type as ScriptClass;
						var method = scrObjType.GetMethod(memberName);
						if (method.Parameters.Count == 1) // The only argument is the object on which it is called
						{
							if (!(i + 3 < CurrentTokens.Count && CurrentTokens[i + 2].Value == "(" && CurrentTokens[i + 3].Value == ")"))
								throw new LsnrParsingException(CurrentTokens[i], "Improperly formatted method call.", Script.Path);
							memberExpression = method.CreateMethodCall
								  (new List<Tuple<string, IExpression>>(), leftExpr);
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
							throw new LsnrParsingException(CurrentTokens[i], $"The type {leftExpr.Type.Name} does not have a field named {memberName}.", Script.Path);
						memberExpression = new FieldAccessExpression(leftExpr, memberName, field.Type);
						nextIndex++; // Skip over the field name.
					}
					else
					{
						throw new LsnrParsingException(CurrentTokens[i], $"The type {leftExpr.Type.Name} does not have a method named {memberName}.", Script.Path);
					}

					var sub = SUB + SubCount++;
					Substitutions.Add(new Token(sub, -1, TokenType.Substitution), memberExpression);
					newTokens.Add(new Token(sub, -1, TokenType.Substitution));
					i = nextIndex - 1; // In the next iteration, i == nextIndex.
					#endregion
				}
				else
				{
					newTokens.Add(CurrentTokens[i]);
				}
			}
			CurrentTokens = newTokens;
		}

		private void ParseParenthesis2()
		{
			var newTokens = new List<Token>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
								// Its' not a function or method
				if (val == "(" && (i < 1 || (CurrentTokens[i - 1].Type != TokenType.Identifier && CurrentTokens[i-1].Value != ">"))) // && (i < 3 || CurrentTokens[i - 2].Value != ".")
				{
					var nextIndex = i + 1;
					var lCount = 1;
					var rCount = 0;
					var j = 1;
					var pTokens = new List<Token>();
					while (lCount != rCount)
					{
						if (CurrentTokens[i + j].Value == ")")
							rCount++;
						else if (CurrentTokens[i + j].Value == "(")
							lCount++;
						if (lCount == rCount) break;
						pTokens.Add(CurrentTokens[i + j]);
						j++;
					}
					var expr = Build(pTokens, Script, Substitutions.Where(p => pTokens.Contains(p.Key)).ToDictionary(), SubCount);
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

		private void ParseIndexers()
		{
			var newTokens = new List<Token>();
			for (int i = 0; i < CurrentTokens.Count; i++)
			{
				var val = CurrentTokens[i].Value;
				if (val == "[")
				{
					var nextIndex = i + 1;
					var balance = -1;

					var j = 1;
					var iTokens = new List<Token>();
					while (balance != 0)
					{
						if (CurrentTokens[i + j].Value == "]")
							balance++;
						else if (CurrentTokens[i + j].Value == "[")
							balance--;
						if (balance == 0) break;
						iTokens.Add(CurrentTokens[i + j]);
						j++;
					}
					var expr = Build(iTokens, Script, Substitutions.Where(p => iTokens.Contains(p.Key)).ToDictionary(), SubCount);
					var name = SUB + SubCount++;
					var coll = GetExpression(newTokens[newTokens.Count - 1]);
					newTokens.RemoveAt(newTokens.Count - 1);
					var t = coll.Type.Type as ICollectionType;
					if (t == null)// typeof(ICollectionType).IsAssignableFrom(coll.Type.GetType()
						throw new LsnrParsingException(CurrentTokens[i], $"{coll.Type.Name} cannot be indexed.", Script.Path);
					if (t.IndexType != expr.Type)
						throw new LsnrParsingException(CurrentTokens[i], $"{coll.Type.Name} cannot be indexed by type {expr.Type.Name}.", Script.Path);
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
					var argType = BinaryExpression.GetArgTypes(left.Type, right.Type);
					IExpression expr = new BinaryExpression(left, right, op, argType);
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

					var argTypes = BinaryExpression.GetArgTypes(left.Type, right.Type);
					IExpression expr = new BinaryExpression(left, right, BinaryOperation.Power, argTypes);
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

				if (op != BinaryOperation.Product)
				{
					var left = GetExpression(CurrentTokens[i - 1]);
					var right = GetExpression(CurrentTokens[i + 1]);
					var argTypes = BinaryExpression.GetArgTypes(left.Type, right.Type);
					IExpression expr = new BinaryExpression(left, right, op, argTypes);
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
					var argTypes = BinaryExpression.GetArgTypes(left.Type, right.Type);
					IExpression expr = new BinaryExpression(left, right, op, argTypes);
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
