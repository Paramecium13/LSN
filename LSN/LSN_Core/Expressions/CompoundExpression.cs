using LSN_Core;
using LSN_Core.Compile;
using LSN_Core.Compile.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	/// <summary>
	/// An expression that consists of component expresions joined together by operators and parenthesis.
	/// </summary>
	[Serializable]
	public class CompoundExpression : Expression
	{

		protected List<IToken> Tokens = new List<IToken>();

		protected Dictionary<IToken, ComponentExpression> Components;

		public override IExpression Fold()
		{
			return this;
		}

		public CompoundExpression(List<IToken> tokens)
		{
			foreach(var token in tokens)
			{
				Tokens.Add(token);
			}
		}

		/// <summary>
		/// Folds all the components
		/// </summary>
		protected void FoldComponents()
		{
			var tempComps = new Dictionary<IToken, ComponentExpression>();
			foreach (var pair in Components)
			{
				tempComps.Add(pair.Key, (ComponentExpression)pair.Value.Fold());
			}
			Components = tempComps;
		}

		/// <summary>
		/// Removes any LiteralExpression components and puts them in the token list.
		/// </summary>
		protected void TrimComponents()
		{
			foreach (var pair in Components)
			{
				if(pair.Value.GetType() == typeof(LSN_Value)
					|| pair.Value.GetType().IsSubclassOf(typeof(LSN_Value)))
				{
					IToken token = TokenFactory.getToken(pair.Value.TranslateUniversal());
					ExtensionMethods.Substitute(Tokens, pair.Key, token);
				}
			}
		}

		public override bool IsReifyTimeConst()
		{
			return Components.Values.All(e => e.IsReifyTimeConst());
		}

		public override ILSN_Value Eval(IInterpreter interpreter)
		{
			StringBuilder str = new StringBuilder();
			for(int i = 0; i < Tokens.Count;)
			{
				str.Append(Tokens[i].Value + " ");
			}
			if(interpreter.PassVariablesByName)
			{
				foreach (var pair in Components.Where(c => c.Value.GetType() != typeof(VariableExpression)))
				{
					str.Replace(pair.Key.Value, pair.Value.Eval(interpreter).TranslateUniversal());
				}
			}
			else
			{
				foreach (var pair in Components)
				{
					str.Replace(pair.Key.Value, pair.Value.Eval(interpreter).TranslateUniversal());
				}
			}
			return interpreter.Eval(str.ToString());
		}
	}
}
