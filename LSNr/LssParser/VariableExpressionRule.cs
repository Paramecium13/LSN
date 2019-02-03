using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LSNr.LssParser
{
	/// <summary>
	/// Creates variable, host interface access, and unique script class expressions.
	/// </summary>
	public class VariableExpressionRule : IExpressionRule
	{
		public uint Priority => ExpressionRulePriorities.Constant;

		public bool CheckToken(Token token, IPreScript script)
		{
			switch (token.Type)
			{
				case TokenType.Identifier:
					switch (script.CheckSymbol(token.Value))
					{
						case SymbolType.Variable:
						case SymbolType.GlobalVariable:
						case SymbolType.UniqueScriptObject:
							return true;
						default:
							return false;
					}
				case TokenType.Keyword:
					var str = token.Value;
					var preScFn = script as PreScriptClassFunction;
					if (str == "self")
					{
						if (script is PreFunction || preScFn != null)
							return true;
						return false; // or throw exception...
					}
					if (str == "host")
					{
						if (preScFn != null)
						{
							if (preScFn.Parent.Host != null)
								return true;
							throw new LsnrParsingException(token, "The keyword 'host' cannot be used in a script class without a host.", script.Path);
						}
						throw new LsnrParsingException(token, "The keyword 'host' cannot be used outside of a script class.", script.Path);
					}
					return false;
				default:
					return false;
			}
		}

		public bool CheckContext(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
			=> true;

		public (IExpression expression, int indexOfNextToken, ushort numTokensToRemoveFromLeft)
			CreateExpression(int index, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions)
		{
			var token = tokens[index];
			var str = token.Value;
			IExpression expr;
			switch (token.Type)
			{
				case TokenType.Identifier:
					switch (script.CheckSymbol(str))
					{
						case SymbolType.Variable:
							var v = script.CurrentScope.GetVariable(str);
							expr = v.AccessExpression;
							//ToDo: Mark that this variable is used here...
							v.MarkAsUsed();
							break;
						case SymbolType.UniqueScriptObject:
							expr = new UniqueScriptObjectAccessExpression(script.GetType(str).Id);
							break;
						case SymbolType.GlobalVariable:
							throw new NotImplementedException();
						default:
							throw new ApplicationException();
					}
					break;
				case TokenType.Keyword:
					if (str == "host")
						expr = new HostInterfaceAccessExpression(((PreScriptClassFunction)script).Parent.HostId);
					else if (str == "self")
						expr = script.CurrentScope.GetVariable("self").AccessExpression;
					else
						throw new ApplicationException();
					break;
				default:
					throw new ApplicationException();
			}
			return (expr, index + 1, 0);
		}
	}
}
