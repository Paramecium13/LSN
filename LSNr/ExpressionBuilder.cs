using LSN_Core;
using LSN_Core.Expressions;
using LSN_Core.Compile;
using LSN_Core.Compile.Tokens;
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
		private IPreScript Script;

		/// <summary>
		/// The variables used in this expression.
		/// </summary>
		private List<Variable> Variables = new List<Variable>();

		private ExpressionBuilder(List<IToken> tokens, IPreScript script)
		{
			InitialTokens = tokens;
			Script = script;
			ParseVariablesAndFunctions();
			//ParseParenthesis();
			//ParseMembersAndInexers();
			//ParseExponents();
			//ParseMultDivMod();
			//ParseAddSubtract();
			//ParseBooleans();//Includes comparisons.
			//...
			//foreach (var v in Variables) v.Users.Add(expr);
		}

		private void ParseVariablesAndFunctions()
		{
			for (int i = 0; i < InitialTokens.Count; i++)
			{
				var val = InitialTokens[i].Value;
				if (Script.CurrentScope.VariableExists(val))
				{
					var v = Script.CurrentScope.GetVariable(val);
					var name = SUB + SubCount++;
					Variables.Add(v);
					Substitutions.Add(new Identifier(name), new VariableExpression(val, v.Type));
					CurrentTokens.Add(new Identifier(name));
				}
				else if (Script.FunctionExists(val)) // It's the start of a function call.
				{
					if (InitialTokens[i + 1].Value != "(") throw new ApplicationException(); // Or throw or log something...
					i += 2;  // Move to the right twice, now looking at token after the opening '('.
					int lCount = 1;
					int rCount = 0;
					var fnTokens = new List<IToken>();
					while (lCount != rCount)
					{
						if (InitialTokens[i].Value == ")")
						{
							lCount++;
							if (lCount == rCount) break;
						}
						else if (InitialTokens[i].Value == "(")
						{
							rCount++;
						}
						fnTokens.Add(InitialTokens[i]);
						i++;
					}
					// Create the function call, add it to the dictionary, and add its identifier to the list (ls).
					var name = SUB + SubCount++;
					var fnCall = Create.CreateFunctionCall(fnTokens, Script.GetFunction(val), Script);
					Substitutions.Add(new Identifier(name), fnCall);
					CurrentTokens.Add(new Identifier(name));
				}
				else
				{
					CurrentTokens.Add(InitialTokens[i]);
				}
			}
		}

		public static IExpression Build(List<IToken> tokens, IPreScript script)
		{
			var b = new ExpressionBuilder(tokens, script);
			return null;
		}

	}
}
