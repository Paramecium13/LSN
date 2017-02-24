﻿using LsnCore;
using Tokens;
using Tokens.Tokens;
using LsnCore.ControlStructures;
using LsnCore.Expressions;
using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	/// <summary>
	/// 
	/// </summary>
	static partial class Create
	{
		/// <summary>
		/// Creates a statement.
		/// </summary>
		/// <param name="tokens"> The tokens of the statement, not including the ';'.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static Statement State(List<IToken> tokens, IPreScript script)
		{
			var v = tokens[0].Value.ToLower();
			int n = tokens.Count;
			if (v == "give")	return Give(tokens, script);
			if (v == "let")		return Assignment(tokens, script);
			if (n > 1 && tokens[1].Value == "=") return Reassignment(tokens, script);
			if (v == "break")	return new BreakStatement();
			if (v == "next")	return new NextStatement();
			if (v == "return")	return new ReturnStatement(n > 1 ? Express(tokens.Skip(1), script) : null);
			if (v == "say")		return Say(tokens.Skip(1).ToList(),script);

			return null;
		}

		/// <summary>
		/// Creates an assignment statement.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="script"></param>
		/// <returns></returns>
		private static AssignmentStatement Assignment(List<IToken> tokens, IPreScript script)
		{
			bool mut = tokens.Any(t => t.Value/*.ToLower()*/ == "mut");
			bool mutable = script.Mutable || mut;
			ushort nameindex = mut ? (ushort)2 : (ushort)1; // The index of the name.
			string name = tokens[nameindex].Value;
			IExpression value = Express(tokens.Skip(nameindex + 2).ToList(), script);
			//LsnType type = value.Type.Type;
			var variable = script.CurrentScope.CreateVariable(name, mutable, value);
			var st = new AssignmentStatement(variable.Index, value);
			variable.Assignment = st;
			return st;
		}

		/// <summary>
		/// Creates a reassignment statement.
		/// </summary>
		/// <param name="tokens"> The tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		private static Statement Reassignment(List<IToken> tokens, IPreScript script)
		{
			if (!script.CurrentScope.VariableExists(tokens[0].Value))
			{
				// The variable does not exist.
				Console.WriteLine($"The variable {tokens[0].Value} does not exist at this point.");
				script.Valid = false;
				return null;
			}
			Variable v = script.CurrentScope.GetVariable(tokens[0].Value);
			if (!v.Mutable)
			{
				// The variable is immutable.
				Console.WriteLine($"The variable {tokens[0].Value} is immutable.");
				script.Valid = false;
				return null;
			}
			IExpression expr = Express(tokens.Skip(2).ToList(), script);
			if (!v.Type.Subsumes(expr.Type.Type))
			{
				Console.WriteLine($"Cannot assign a value of type {expr.Type.Name} to a variable ({v.Name}) of type {v.Type.Name}.");
				script.Valid = false;
				return null;
			}
			var reassign = new ReassignmentStatement(v.Index, expr);
			v.AddReasignment(reassign);
			return reassign;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"> The tokens of the statement; without the 'say' and ';' tokens.</param>
		/// <param name="script"></param>
		/// <returns></returns>
		private static SayStatement Say(List<IToken> tokens, IPreScript script)
		{
			IExpression message, graphic = null, title = null;
			if(tokens.HasToken("with") || tokens.HasToken("withgraphic"))
			{
				int withIndex = tokens.IndexOf("with");
				if (withIndex < 0) withIndex = tokens.IndexOf("withgraphic");
				if (tokens.HasToken("as"))
				{
					int asIndex = tokens.IndexOf("as");
					int firstIndex = Math.Min(withIndex, asIndex);
					int secondIndex = Math.Max(withIndex, asIndex);
                    message = Express(tokens.Take(firstIndex),script);
					var expr2 = Express(tokens.Skip(firstIndex).Take(secondIndex - firstIndex),script);
					var expr3 = Express(tokens.Skip(secondIndex), script);
					if(firstIndex == withIndex) { graphic = expr2; title = expr3; }
					else{ title = expr2; graphic = expr3; }
				}
				else
				{
					message = Express(tokens.Take(withIndex), script);
					graphic = Express(tokens.Skip(withIndex), script);
				}
			}
			else if(tokens.HasToken("as"))
			{
				int asIndex = tokens.IndexOf("as");
				message = Express(tokens.Take(asIndex), script);
				title = Express(tokens.Skip(asIndex),script);
			}
			else // No title or graphic
			{
				message = Express(tokens,script);
			}
			return new SayStatement(message,graphic,title);
		}

		/// <summary>
		/// Used in making give statements.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="str"></param>
		/// <param name="indexOfString"></param>
		/// <returns></returns>
		private static IExpression GetExpression(IEnumerable<IToken> tokens, string str, out int indexOfString, IPreScript script)
		{
			indexOfString = tokens.Select(t => t.Value.ToLower()).ToList().IndexOf(str);
			List<IToken> exprTokens = tokens.Take(indexOfString - 1).ToList();
			return Express(exprTokens, script);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		private static GiveStatement Give(List<IToken> tokens, IPreScript script)
		{
			if (tokens.Count < 2) throw new NotImplementedException();
			if (tokens.Any(t => t.Value == "item"))
			{
				return GiveItem(tokens, script);
			}
			else if (tokens.Any(t => t.Value == "gold"))
			{
				return GiveGold(tokens, script);
			}
			/*else if (tokens.Any(t => t.Value.ToLower() == "armor" || t.Value.ToLower() == "armour"))
			{
				return GiveArmor(tokens, script);
			}
			else if (tokens.Any(t => t.Value.ToLower() == "weapon"))
			{
				return GiveWeapon(tokens, script);
			}
			*/
			else
				return null;
		}

		private static GiveItemStatement GiveItem(List<IToken> tokens, IPreScript script)
		{
			int index1, index2;
			IExpression Amount;

			if (tokens[2].Value == "item")
			{
				Amount = new LsnValue(1);
				index1 = 2;
			}
			else
				Amount = GetExpression(tokens.Skip(1), "item", out index1, script);
			IExpression Id;
			IExpression receiver = LsnValue.Nil;
			var idTokens = tokens.Skip(index1 + 1);
			if (idTokens.Any(t => t.Value == "to"))
			{
				Id = GetExpression(idTokens, "to", out index2, script);
				receiver = Express(idTokens.Skip(index2 + 1), script);
			}
			else
			{
				Id = Express(idTokens, script);
			}
			return new GiveItemStatement(Id, Amount,receiver);
		}

		private static GiveGoldStatement GiveGold(List<IToken> tokens, IPreScript script)
		{
			IExpression Amount;
			IExpression receiver = LsnValue.Nil;
			
			int indexOfKeywordGold = tokens.Select(t => t.Value).ToList().IndexOf("gold");
			if(tokens.Any(t => t.Value == "to"))
			{
				int i;
				Amount = GetExpression(tokens, "to", out i, script);
				receiver = Express(tokens.Skip(i + 1), script);
			}
			else
				Amount = Express(tokens.Skip(1).Take(indexOfKeywordGold - 1).ToList(), script);
			return new GiveGoldStatement(Amount,receiver);
		}
	}
}