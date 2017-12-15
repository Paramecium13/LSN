using LsnCore;
using Tokens;
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
	/// ...
	/// </summary>
	internal static partial class Create
	{
		/// <summary>
		/// Creates a statement.
		/// </summary>
		/// <param name="tokens"> The tokens of the statement, not including the ';'.</param>
		/// <param name="script"> The script.</param>
		/// <returns></returns>
		public static Statement State(List<Token> tokens, IPreScript script)
		{
			var v = tokens[0].Value.ToLower();
			int n = tokens.Count;
			if (v == "give")
				return Give(tokens, script);
			if (v == "let")
				return Assignment(tokens, script);
			if (n > 1 && tokens[1].Value == "=")
								return Reassignment(tokens, script);
			if (v == "break")	return new BreakStatement();
			if (v == "next")	return new NextStatement();
			if (v == "return")	return new ReturnStatement(n > 1 ? Express(tokens.Skip(1), script) : null);
			if (v == "say")
				return Say(tokens.Skip(1).ToList(),script);
			if (v == "goto")
				return GotoStatement(tokens,script);
			if (v == "setstate")
			{
				if (tokens.Count != 2)
					throw new LsnrParsingException(tokens[0],"Improperly formated setstate statement. Correct format is: 'setstate statename;'.",script.Path);

				var stateName = tokens[1].Value;
				var preScObjFn = script as PreScriptObjectFunction;
				if (preScObjFn == null)
					throw new LsnrParsingException(tokens[0],"Cannot use a setstate statement outside of a script object.",script.Path); // Cannot use SetState here.

				var preScObj = preScObjFn.Parent;

				if (!preScObj.StateExists(stateName))
					throw new LsnrParsingException(tokens[1],$"The state '{stateName}' does not exist.",script.Path); // State does not exist.
				return new SetStateStatement(preScObj.GetStateIndex(stateName));
			}

			if (tokens.Any(t => t.Value == "goto"))
				return GotoStatement(tokens, script);

			if (tokens.Any(t => t.Value == "="))
			{
				var leftTokens = tokens.TakeWhile(t => t.Value != "=").ToList();
				var rightTokens = tokens.Skip(leftTokens.Count + 1).ToList();
				var left = Express(leftTokens, script);
				var right = Express(rightTokens, script);
				var field = left as FieldAccessExpression;
				var collection = left as CollectionValueAccessExpression;
				if (field != null)
				{
					if (field.Type != right.Type)
						throw LsnrParsingException.TypeMismatch(tokens[leftTokens.Count], left.Type.Name, right.Type.Name, script.Path);
					return new FieldAssignmentStatement(field.Value, field.Index, right);
				}
				if (collection != null)
				{
					if (collection.Type != right.Type)
						throw LsnrParsingException.TypeMismatch(tokens[leftTokens.Count], left.Type.Name, right.Type.Name, script.Path);
					return new CollectionValueAssignmentStatement(collection.Collection, collection.Index, right);
				}
				throw new LsnrParsingException(tokens[leftTokens.Count], "Improper assignment.", script.Path);
			}
			// Assignment of a field or an element in a collection. May have multiple levels, e.g. 'a.b[i-1].x = 42;'
			// Parse both sides of the '=' as expressions. The left side should be either a field access expression or a
			// collection value access expression. Parse this expression. If it is a field access expression, take the 'Value' and
			// the field index. ... If it is a collection value access expression, take the 'Value' expression and the 'Index' expression.

			// Expression statement:
			// When all else fails, parse the whole thing as an expression.
			return new ExpressionStatement(Express(tokens, script));
			// The top level expression should be a function call, method call, ScriptObjectMethodCall, or HostInterfaceMethodCall.
			// If it isn't, complain.

			throw new LsnrParsingException(tokens[0], "Could not parse statement.", script.Path);
		}

		/// <summary>
		/// Creates an assignment statement.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="script"></param>
		/// <returns></returns>
		private static Statement Assignment(List<Token> tokens, IPreScript script)
		{
			bool mut = tokens.Any(t => t.Value/*.ToLower()*/ == "mut");
			bool mutable = script.Mutable || mut;
			ushort nameindex = mut ? (ushort)2 : (ushort)1; // The index of the name.
			string name = tokens[nameindex].Value;
			IExpression value = Express(tokens.Skip(nameindex + 2).ToList(), script);
			//LsnType type = value.Type.Type;
			var symType = script.CheckSymbol(name);
			switch (symType)
			{
				case SymbolType.UniqueScriptObject:
				case SymbolType.Variable:
				case SymbolType.GlobalVariable:
				case SymbolType.Field:
				case SymbolType.Property:
				case SymbolType.ScriptObjectMethod:
				case SymbolType.HostInterfaceMethod:
				case SymbolType.Function:
					throw new LsnrParsingException(tokens[nameindex],$"Cannot name a new variable '{name}'. That name is already used for a {symType.ToString()}.",script.Path);
				case SymbolType.Undefined:
					break;
			}
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
		private static Statement Reassignment(List<Token> tokens, IPreScript script)
		{
			var sy = script.CheckSymbol(tokens[0].Value);
			var expr = Express(tokens.Skip(2).ToList(), script);
			switch (sy)
			{
				case SymbolType.Variable:
					var v = script.CurrentScope.GetVariable(tokens[0].Value);
					if (!v.Mutable)
					{
						// The variable is immutable.
						/*Console.WriteLine($"The variable {tokens[0].Value} is immutable.");
						script.Valid = false;
						return null;*/
						throw new LsnrParsingException(tokens[0], $"The variable {tokens[0].Value} is immutable.", script.Path);
					}
					if (!v.Type.Subsumes(expr.Type.Type))
					{
						/*Console.WriteLine($"Cannot assign a value of type '{expr.Type.Name}' to  variable '{v.Name}' of type '{v.Type.Name}'.");
						script.Valid = false;
						return null;*/
						throw new LsnrParsingException(tokens[0], $"Cannot assign a value of type '{expr.Type.Name}' to  variable '{v.Name}' of type '{v.Type.Name}'.", script.Path);
					}
					var assign = new AssignmentStatement(v.Index, expr);
					v.AddReasignment(assign);
					return assign;
				case SymbolType.Field: // This is inside a script object...
					return new FieldAssignmentStatement(new VariableExpression(0,(script as PreScriptObjectFunction).Parent.Id),
						(script as PreScriptObjectFunction).Parent.GetField(tokens[0].Value).Index,
						expr);
				case SymbolType.GlobalVariable:
					throw new NotImplementedException("");
				case SymbolType.Property:
					throw new LsnrParsingException(tokens[0], $"Attempt to modify property \"{tokens[0].Value}\"; properties are immutable.", script.Path);
				case SymbolType.Undefined:
					throw new LsnrParsingException(tokens[0], $"The symbol \"{tokens[0].Value}\" is undefined.", script.Path);
				case SymbolType.ScriptObjectMethod:
				case SymbolType.HostInterfaceMethod:
				case SymbolType.Function:
				default:
					throw new LsnrParsingException(tokens[0], "Cannot use a method or function as a variable.", script.Path);
			}
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="tokens"> The tokens of the statement; without the 'say' and ';' tokens.</param>
		/// <param name="script"></param>
		/// <returns></returns>
		private static SayStatement Say(List<Token> tokens, IPreScript script)
		{
			IExpression message, graphic = LsnValue.Nil, title = LsnValue.Nil;
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
		/// <param name="script"></param>
		/// <returns></returns>
		private static IExpression GetExpression(IEnumerable<Token> tokens, string str, out int indexOfString, IPreScript script)
		{
			indexOfString = tokens.Select(t => t.Value.ToLower()).ToList().IndexOf(str);
			List<Token> exprTokens = tokens.Take(indexOfString - 1).ToList();
			return Express(exprTokens, script);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="script"></param>
		/// <returns></returns>
		private static GiveStatement Give(List<Token> tokens, IPreScript script)
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

		private static GiveItemStatement GiveItem(List<Token> tokens, IPreScript script)
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
			return new GiveItemStatement(Id, Amount, receiver);
		}

		private static GiveGoldStatement GiveGold(List<Token> tokens, IPreScript script)
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


		private static Statement GotoStatement(IReadOnlyList<Token> tokens, IPreScript script)
		{
			IExpression actor = null;
			IReadOnlyList<Token> tokens0 = null;
			if (tokens[0].Value == "goto")
				tokens0 = tokens;
			else
			{
				var actorTokens = tokens.TakeWhile(t => t.Value != "goto").ToList();
				tokens0 = tokens.Skip(actorTokens.Count + 1).ToList();
				actor = Express(actorTokens, script);
			}

			var metaCommaCount = tokens0.Count(t => t.Value == "`");
			IExpression expr0 = null;
			IExpression expr1 = null;
			IExpression expr2 = null;
			switch (metaCommaCount)
			{
				case 0:
					expr0 = Express(tokens0, script);
					break;
				case 1:
					{
						var tokens1 = tokens0.TakeWhile(t => t.Value != "`").ToList();
						var tokens2 = tokens0.Skip(tokens1.Count + 1).ToList();
						expr0 = Express(tokens1, script);
						expr1 = Express(tokens2, script);
						break;
					}
				case 2:
					{
						var tokens1 = tokens0.TakeWhile(t => t.Value != "`").ToList();
						var tokens2 = tokens0.Skip(tokens1.Count + 1).TakeWhile(t => t.Value != "`").ToList();
						var tokens3 = tokens0.Skip(tokens1.Count + tokens2.Count + 2).ToList();
						expr0 = Express(tokens1, script);
						expr1 = Express(tokens2, script);
						expr2 = Express(tokens3, script);
						break;
					}
				default:
					throw new LsnrParsingException(tokens.First(t=>t.Value.ToLower() == "goto"), "Improperly formated goto statement (considered harmful).", script.Path);
			}
			return new GoToStatement(expr0, expr1, expr2, actor);
		}

	}
}