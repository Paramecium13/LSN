using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LSNr.ScriptObjects;

namespace LSNr.LssParser
{
	public class PropertyFieldRule : IExpressionRule
	{
		public uint Priority => ExpressionRulePriorities.Constant;

		public bool CheckToken(Token token, IPreScript script)
		{
			if(token.Type == TokenType.Identifier)
			{
				switch (script.CheckSymbol(token.Value))
				{
					case SymbolType.Field:
					case SymbolType.Property:
						return true;
					default:
						return false;
				}
			}
			return false;
		}

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			if (index < 2)
				return true;
			return tokens[index - 1].Value != ".";
		}

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var token = tokens[index];
			var str = token.Value;
			IExpression expr;
			var preScriptClass = (IBasePreScriptClass)((IPreFunction)script).Parent;
			var self = script.CurrentScope.GetVariable("self").AccessExpression;
			switch (script.CheckSymbol(str))
			{
				case SymbolType.Field:
					expr = new FieldAccessExpression(self, preScriptClass.GetField(str));
					break;
				/*case SymbolType.Property:
					expr = new PropertyAccessExpression(self, preScriptClass.GetPropertyIndex(str), preScriptClass.GetProperty(str).Type);
					break;*/
				default:
					throw new ApplicationException();
			}
			return (expr, index + 1, 0);
		}
	}
}
