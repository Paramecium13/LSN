using LSN_Core;
using LSN_Core.Compile;
using LSN_Core.Compile.Tokens;
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
		/// <summary>
		/// Creates a control structure.
		/// </summary>
		/// <param name="head"> The head tokens.</param>
		/// <param name="body"> The body tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static ControlStructure ControlStructure(List<IToken> head, List<IToken> body, PreScript script)
		{
			string h = head[0].Value;
			if (h == "if")
			{
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
                script.CurrentScope = script.CurrentScope.Pop(components);
				return new IfControl(Express(head.Skip(1).ToList(), script), components);
			}
			else if (h == "elsif")
			{
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new ElsifControl(Express(head.Skip(1).ToList(),script), components);
			}
			else if (h == "else")
			{
				script.CurrentScope = new Scope(script.CurrentScope);
				Parser p = new Parser(body,script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new ElseControl(Express(head.Skip(1).ToList(), script), components);
			}
			else if(h == "choice")
			{
				// It's a choice block.
			}
			else if(h == "?")
			{
				// It's a conditional choice (inside a choice block).
			}
			else if(head.Count > 1 && head[1].Value == "->")
			{
				// It's a choice (inside a choice block).
			}
			return null;
		}

		/// <summary>
		/// Create an expression.
		/// </summary>
		/// <param name="list"> The list of tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static Expression Express(List<IToken> list, PreScript script)
		{
			if (list[0].Value.ToLower() == "get")
			{
				return CreateGet(list,script);
			}
			else if(list.Count == 1)
			{
				if(script.CurrentScope.VariableExists(list[0].Value))
				{
					var v = script.CurrentScope.GetVariable(list[0].Value);
					return new VariableExpression(v.Name, v.Type);
				}
				else if (list[0].GetType() == typeof(FloatToken))
				{
					return new ValueExpression(new DoubleValue(((FloatToken)list[0]).DVal));
				}
				else if (list[0].GetType() == typeof(IntToken))
				{
					return new ValueExpression(new IntValue(((IntToken)list[0]).IVal));
				}
				else if (list[0].GetType() == typeof(StringToken))
				{
					return new ValueExpression(new StringValue(list[0].Value));
				}
				else
				{
					Console.WriteLine($"Cannot parse expression {list[0]}.");
					script.Valid = false;
					return null; // Use a "Null expression" here instead so the reifier can check for more errors.
				}
			}
			else
			{
				return Compound(list, script);
			}
		}

		/// <summary>
		/// Create a compound expression.
		/// </summary>
		/// <param name="list"> The list of tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns> ...</returns>
		private static CompoundExpression Compound(List<IToken> list, PreScript script)
		{
			var dict = new Dictionary<IToken, ComponentExpression>();
			var vars = new List<Variable>();
			foreach(var token in list)
			{
				if(script.CurrentScope.VariableExists(token.Value))
				{
					var v = script.CurrentScope.GetVariable(token.Value);
                    vars.Add(v);
					dict.Add(token, new VariableExpression(token.Value, v.Type));
				}
			}
			var expr = new CompoundExpression(list, dict);
			foreach (var v in vars) v.Users.Add(expr);
			return expr;
		}

		public static Expression CreateGet(List<IToken> tokens, PreScript script)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates a statement.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="script"></param>
		/// <returns></returns>
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
			else if (tokens.Count > 1 && tokens[1].Value == "=")
			{
				return Reassignment(tokens,script);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Creates an assignment statement.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="script"></param>
		/// <returns></returns>
		private static AssignmentStatement Assignment(List<IToken> tokens, PreScript script)
		{
			bool mut = tokens.Any(t => t.Value.ToLower() == "mut");
			bool mutable = script.Mutable || mut;
			ushort nameindex = mut ? (ushort)2 : (ushort)1; // The index of the name.
			string name = tokens[nameindex].Value;
			IExpression value = Express(tokens.Skip(nameindex + 2).ToList(), script);
			LSN_Type type = value.Type;
			var st = new AssignmentStatement(name, value);
            var v = new Variable(name, mutable, value, st);
			script.CurrentScope.AddVariable(v);
			return st;
		}

		/// <summary>
		/// Creates a reassignment statement.
		/// </summary>
		/// <param name="tokens"> The tokens.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		private static Statement Reassignment(List<IToken> tokens, PreScript script)
		{
			if(! script.CurrentScope.VariableExists(tokens[0].Value))
			{
				// The variable does not exist.
				Console.WriteLine($"The variable {tokens[0].Value} does not exist at this point.");
				script.Valid = false;
				return null;
			}
			if(! script.CurrentScope.GetVariable(tokens[0].Value).Mutable)
			{
				// The variable is immutable.
				Console.WriteLine($"The variable {tokens[0].Value} is immutable.");
				script.Valid = false;
				return null;
			}
			Variable v = script.CurrentScope.GetVariable(tokens[0].Value);
            IExpression expr = Express(tokens.Skip(2).ToList(), script);
			if(! v.Type.Subsumes(expr.Type))
			{
				Console.WriteLine($"Cannot assign a value of type {expr.Type.Name} to a variable ({v.Name}) of type {v.Type.Name}.");
				script.Valid = false;
				return null;
			}
			var re = new ReassignmentStatement(v.Name, expr);
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
