using LSN_Core;
using LSN_Core.Compile;
using LSN_Core.ControlStructures;
using LSN_Core.Expressions;
using LSN_Core.Statements;
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
	static class Create
	{
		public static ControlStructure ControlStructure(List<IToken> head, List<IToken> body, PreScript script)
		{
			string h = head[0].Value;
			if (h == "if")
			{
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body, script);
				p.Parse();
				script.CurrentScope = script.CurrentScope.Pop();
				return new IfControl(Express(head.Skip(1).ToList(),script), Parser.Consolidate(p.Components));
			}
			else if (h == "elsif")
			{
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body, script);
				p.Parse();
				script.CurrentScope = script.CurrentScope.Pop();
				return new ElsifControl(Express(head.Skip(1).ToList(),script), Parser.Consolidate(p.Components));
			}
			else if (h == "else")
			{
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body,script);
				p.Parse();
				script.CurrentScope = script.CurrentScope.Pop();
				return new ElseControl(Express(head.Skip(1).ToList(), script), Parser.Consolidate(p.Components));
			}
			return null;
		}

		public static Expression Express(List<IToken> list, PreScript script)
		{
			if (list[0].Value.ToLower() == "get")
			{
				return CreateGet(list,script);
			}else
			{
				return Compound(list, script);
			}
		}

		private static CompoundExpression Compound(List<IToken> list, PreScript script)
		{
			throw new NotImplementedException();
		}

		public static Expression CreateGet(List<IToken> tokens, PreScript script)
		{
			return null;
		}


		public static Statement State(List<IToken> tokens, PreScript script)
		{
			if (tokens[0].Value.ToLower() == "give")
			{
				return Give(tokens,script);
			}
			else if (tokens[0].Value.ToLower() == "let")
			{
				return Assignment(tokens,script);
			}
			else if (tokens[1].Value == "=")
			{
				return Reassignment(tokens,script);
			}
			else
			{
				return null;
			}
		}

		private static AssignmentStatement Assignment(List<IToken> tokens, PreScript script)
		{
			bool mut = tokens.Any(t => t.Value.ToLower() == "mut");
			bool mutable = script.Mutable || mut;
			ushort nameindex = mut ? (ushort)2 : (ushort)1; // The index of the name.
			string name = tokens[nameindex].Value;
			IExpression value = Express(tokens.Skip(nameindex + 2).ToList(), script);
			LSN_Type type = value.Type;
			var v = new Variable(name, type, mutable, value);
			script.CurrentScope.AddVariable(v);
			return new AssignmentStatement(name, value);
			throw new NotImplementedException();
		}

		private static Statement Reassignment(List<IToken> tokens, PreScript script)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Used in making give statements.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="str"></param>
		/// <param name="indexOfString"></param>
		/// <returns></returns>
		private static IExpression GetExpression(IEnumerable<IToken> tokens, string str, out int indexOfString, PreScript script)
		{
			indexOfString = tokens.Select(t => t.Value.ToLower()).ToList().IndexOf(str);
			List<IToken> exprTokens = tokens.Take(indexOfString - 1).ToList();
			return Express(exprTokens,script);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		public static GiveStatement Give(List<IToken> tokens, PreScript script)
		{
			if (tokens.Any(t => t.Value.ToLower() == "item"))
			{
				return  GiveItem(tokens, script);
			}
			else if (tokens.Any(t => t.Value.ToLower() == "armor" || t.Value.ToLower() == "armour"))
			{
				return  GiveArmor(tokens, script);
			}
			else if (tokens.Any(t => t.Value.ToLower() == "weapon"))
			{
				return  GiveWeapon(tokens, script);
			}
			else if (tokens.Any(t => t.Value.ToLower() == "g"))
			{
				return GiveGold(tokens, script);
			}
			else
				return null;
		}

		private static GiveItemStatement GiveItem(List<IToken> tokens, PreScript script)
		{
			int index1, index2;
			var Amount = GetExpression(tokens.Skip(1), "item", out index1,script);
			var Id = GetExpression(tokens.Skip(index1 + 1), "to", out index2,script);
			return new GiveItemStatement(Id, Amount);
		}

		private static GiveGoldStatement GiveGold(List<IToken> tokens, PreScript script)
		{
			int x = tokens.Select(t => t.Value).ToList().IndexOf("G");
			var exprTokens = tokens.Skip(1).Take(x - 1).ToList();
			var Amount = Express(exprTokens,script);
			return new GiveGoldStatement(Amount);
		}

		private static GiveWeaponStatement GiveWeapon(List<IToken> tokens, PreScript script)
		{
			//Id = GetExpression(tokens, "weapon");
			int x = tokens.Select(t => t.Value.ToLower()).ToList().IndexOf("weapon");
			var exprTokens = tokens.Skip(1).Take(x - 1).ToList();
			var Amount = Express(exprTokens,script);
			var Id = Express(tokens.Skip(x + 1).ToList(),script);
			return new GiveWeaponStatement(Id, Amount);
		}

		private static GiveArmorStatement GiveArmor(List<IToken> tokens, PreScript script)
		{
			//Id = GetExpression(tokens,"armor");
			int x;
			if (tokens.Any(t => t.Value == "armor"))
				x = tokens.Select(t => t.Value.ToLower()).ToList().IndexOf("armor");
			else x = tokens.Select(t => t.Value.ToLower()).ToList().IndexOf("armour");
			//The tokens before the token 'armor'
			var exprTokens = tokens.Skip(1).Take(x - 1).ToList();
			var Amount = Express(exprTokens, script);
			var Id = Express(tokens.Skip(x + 1).ToList(),script);
			return new GiveArmorStatement(Id, Amount);
		}
	}
}
