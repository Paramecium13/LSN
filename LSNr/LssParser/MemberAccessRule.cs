using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Expressions;

namespace LSNr.LssParser
{
	public class MemberAccessRule : IExpressionRule
	{
		public uint Priority => ExpressionRulePriorities.MemberAccess;

		public bool CheckToken(Token token, IPreScript script)
			=> token.Value == ".";

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			if (index == 0)
				throw new LsnrParsingException(tokens[0], "Expression cannot start with '.'", script.Path);
			return true;
		}

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			IExpression expr = null;
			ushort numLeft = 1;
			int nextIndex = -1;

			var left = tokens[index - 1];
			var right = tokens[index + 1];
			var rightStr = right.Value;
			IExpression leftExpr;
			switch (left.Type)
			{
				case TokenType.SyntaxSymbol:
					throw new NotImplementedException();
				case TokenType.Substitution:
					leftExpr = substitutions[left];
					numLeft = 1;
					break;
				default:
					throw new NotImplementedException("Error: Error not yet implemented.");
			}

			// Is this a method, a host interface method, a script class method, a property, or a field?
			var hiType		= leftExpr.Type.Type	as HostInterfaceType;
			var scType		= leftExpr.Type.Type	as ScriptClass;
			var fieldType	= leftExpr.Type.Type	as IHasFieldsType;

			if (hiType != null) // It's a host interface method call.
			{
				if (!hiType.HasMethod(rightStr))
					throw new LsnrParsingException(right, "...", script.Path);
				var def = hiType.MethodDefinitions[rightStr];
				IExpression[] args = null;

				if (def.Parameters.Count == 0)
				{
					args = new IExpression[0];
					if (index + 2 < tokens.Count && tokens[index + 2].Value == "(")
					{
						if (index + 3 >= tokens.Count || tokens[index + 3].Value != ")")
							throw new LsnrParsingException(tokens[index + 2], "...", script.Path);
						nextIndex = index + 4; // Skip over the parenthesis.
					}
				}
				else
				{
					var x = Create.CreateArgList(index + 2, tokens, script);
					nextIndex = x.indexOfNextToken;
					var argTokens = x.argTokens;

					args = argTokens
						.Select(a => ExpressionParser.Parse(a, script, substitutions))
						.ToArray();
				}

				expr = new HostInterfaceMethodCall(def, leftExpr, args);
			}

			else if (scType != null) // It could be a property access expression
			{
				var props = scType.Properties.Where(p => p.Name == rightStr).ToArray();
				if(props.Length != 0)
				{
					expr = new PropertyAccessExpression(leftExpr, scType.GetPropertyIndex(rightStr));
					nextIndex = index + 2;
				}
			}

			if (expr == null && fieldType != null) // It could be a field access expression.
			{
				var arr = fieldType.FieldsB.Where(f => f.Name == rightStr).ToArray();
				if (arr.Length != 0)
				{
					nextIndex = index + 2;
					expr = new FieldAccessExpression(leftExpr, arr[0]);	// ToDo: Encapsulation...
				}
			}

			if(expr == null) // It's a method call.
			{
				var args = new IExpression[] { leftExpr };
				var type = leftExpr.Type.Type;
				if (!type.Methods.ContainsKey(rightStr))
					throw new LsnrParsingException(right, "...", script.Path);

				var method = type.Methods[rightStr];
				if (method.Parameters.Count == 0 && index + 2 < tokens.Count && tokens[index + 2].Value == "(")
				{
					if (index + 3 >= tokens.Count || tokens[index + 3].Value != ")")
						throw new LsnrParsingException(tokens[index + 2], "...", script.Path);
					nextIndex = index + 4; // Skip over the parenthesis.
				}
				else
				{
					var x = Create.CreateArgList(index + 2, tokens, script);
					var argTokens = x.argTokens;
					nextIndex = x.indexOfNextToken;

					var a = new List<IExpression>(method.Parameters.Count);
					a.Add(args[0]);
					a.AddRange(argTokens.Select(ar => ExpressionParser.Parse(ar, script, substitutions)));
					args = a.ToArray();
				}
				expr = new MethodCall(method, args);

			}

			return (expr, nextIndex, numLeft);
		}
	}
}
