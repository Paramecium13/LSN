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
			var nextIndex = -1;

			var left = tokens[index - 1];
			var right = tokens[index + 1];
			var memberName = right.Value;
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
			HostInterfaceType hiType;
			try
			{
				hiType		= leftExpr.Type.Type	as HostInterfaceType;
			}
			catch (Exception)
			{
				throw;
			}
			var scType		= leftExpr.Type.Type	as ScriptClass;
			var fieldType	= leftExpr.Type.Type	as IHasFieldsType;

			IExpression[] args;
			if (hiType != null) // It's a host interface method call.
			{
				if (!hiType.HasMethod(memberName))
					throw new LsnrParsingException(right, "...", script.Path);
				var def = hiType.MethodDefinitions[memberName];

				if (def.Parameters.Count == 0)
				{
					args = Array.Empty<IExpression>();
					if (index + 2 < tokens.Count && tokens[index + 2].Value == "(")
					{
						if (index + 3 >= tokens.Count || tokens[index + 3].Value != ")")
							throw new LsnrParsingException(tokens[index + 2], "...", script.Path);
						nextIndex = index + 4; // Skip over the parenthesis.
					}
				}
				else
				{
					var (argTokens, indexOfNextToken) = Create.CreateArgList(index + 2, tokens, script);
					nextIndex = indexOfNextToken;

					args = argTokens
						.Select(a => ExpressionParser.Parse(a, script, substitutions))
						.ToArray();
				}

				expr = new HostInterfaceMethodCall(def, leftExpr, args);
			}

			/*else if (scType != null) // It could be a property access expression
			{
				var props = scType.Properties.Where(p => p.Name == memberName).ToArray();
				if(props.Length != 0)
				{
					expr = new PropertyAccessExpression(leftExpr, scType.GetPropertyIndex(memberName));
					nextIndex = index + 2;
				}
			}*/

			if (expr == null && fieldType != null) // It could be a field access expression.
			{
				var arr = fieldType.FieldsB.Where(f => f.Name == memberName).ToArray();
				if (arr.Length != 0)
				{
					if (scType != null && !(script is PreScriptClassFunction && leftExpr is VariableExpression v && v.Index == 0))
						throw new LsnrParsingException(tokens[index], "A script object's fields can only be accessed by that script object.", script.Path);
					nextIndex = index + 2;
					expr = new FieldAccessExpression(leftExpr, arr[0]);
				}
			}

			if (expr != null) return (expr, nextIndex, numLeft);
			
			args = new[] { leftExpr };
			var type = leftExpr.Type.Type;
			if (!type.Methods.ContainsKey(memberName))
				throw new LsnrParsingException(right, $"'{leftExpr.Type.Name}' does not have a method (or any member) named '{memberName}'.", script.Path);

			var method = type.Methods[memberName];
			if (method.Parameters.Count == 1 && index + 2 < tokens.Count && tokens[index + 2].Value == "(")
			{
				if (index + 3 >= tokens.Count || tokens[index + 3].Value != ")")
					throw new LsnrParsingException(tokens[index + 2], "...", script.Path);
				nextIndex = index + 4; // Skip over the parenthesis.
			}
			else if(method.Parameters.Count > 1)
			{
				(args, nextIndex) = Utilities.Parameters.CreateArgs(index + 2, tokens, method.TypeId + "::" + memberName, method.Parameters, script,
					leftExpr, substitutions);
			}
			expr = method.CreateMethodCall(args);
			
			return (expr, nextIndex, numLeft);
		}
	}
}
