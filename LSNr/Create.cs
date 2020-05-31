using LsnCore;
using LsnCore.ControlStructures;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;
using LsnCore.Values;
using LSNr.LssParser;
using LSNr.ScriptObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSNr
{
	/// <summary>
	/// ...
	/// </summary>
	static partial class Create
	{
		/// <summary>
		/// Create an <see cref="IExpression"/> from the provided <see cref="Token"/>s.
		/// </summary>
		/// <param name="tokens">The tokens.</param>
		/// <param name="script">The script.</param>
		/// <returns></returns>
		public static IExpression Express(IEnumerable<Token> tokens, IPreScript script/*, IExpressionContainer container*/)
			=> Express(tokens.ToList(), script);

		/// <summary>
		/// Create an <see cref="IExpression"/> from the provided <see cref="Token"/>s.
		/// </summary>
		/// <param name="list"> The list of tokens.</param>
		/// <param name="script"> The script.</param>
		/// <param name="substitutions">todo: describe substitutions parameter on Express</param>
		/// <returns></returns>
		public static IExpression Express(IReadOnlyList<Token> list, IPreScript script, IReadOnlyDictionary<Token,IExpression> substitutions = null)
		{
			if (list.Count != 1) return ExpressionParser.Parse(list, script, substitutions).Fold();
			var token = list[0];
			if (substitutions != null && substitutions.ContainsKey(token))
				return substitutions[token];
			var vars = new List<Variable>();
			var expr = SingleTokenExpress(token, script, null, vars);
			if(vars.Count != 0)
				vars[0].AddUser(expr);

			return expr;
		}

		/// <summary>
		/// Creates an <see cref="IExpression"/> from a single <see cref="Token"/>.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="script">The script.</param>
		/// <param name="container">The container.</param>
		/// <param name="variables">The variables.</param>
		public static IExpression SingleTokenExpress(Token token, IPreScript script, IExpressionContainer container = null, IList<Variable> variables = null)
		{
			var val = token.Value;
			var symType = script.CheckSymbol(val);
			var preScrFn = script as IPreFunction;
			IBasePreScriptClass preScCl = null;
			if (preScrFn != null)
				preScCl = preScrFn.Parent as IBasePreScriptClass;
			switch (symType)
			{
				case SymbolType.Variable:
					var v = script.CurrentScope.GetVariable(val);
					if (!v.Mutable && (v.InitialValue?.IsReifyTimeConst() ?? false))
						return v.InitialValue.Fold();
					var expr = v.AccessExpression;
					if (container != null)
						v.AddUser(container);
					else
						variables?.Add(v);
					return expr;
				case SymbolType.UniqueScriptObject:
					return new UniqueScriptObjectAccessExpression(script.GetTypeId(val));
				case SymbolType.GlobalVariable:
					throw new NotImplementedException();
				case SymbolType.Field:
					if (preScCl == null)
						throw new ApplicationException("...");
					return new FieldAccessExpression(preScrFn.CurrentScope.GetVariable("self").AccessExpression, preScCl.GetField(val));
				/*case SymbolType.Property:
					var preScr = preScrFn.Parent;
					return new PropertyAccessExpression(preScrFn.CurrentScope.GetVariable("self").AccessExpression, preScr.GetPropertyIndex(val), preScr.GetProperty(val).Type);*/
				default:
					break;
			}
			switch (token.Type)
			{
				case TokenType.Float:
					return new LsnValue(token.DoubleValue);
				case TokenType.Integer:
					return new LsnValue(token.IntValue);
				case TokenType.String:
					return new LsnValue(new StringValue(token.Value));
				case TokenType.Substitution:
					throw new ApplicationException();
				default:
					break;
			}

			switch (val)
			{
				case "true":
					return LsnBoolValue.GetBoolValue(true);
				case "false":
					return LsnBoolValue.GetBoolValue(false);
				case "self":
				{
					if (preScrFn == null)
						throw new LsnrParsingException(token, "Cannot use 'this' outside a script object method or event listener.", script.Path);
					return script.CurrentScope.GetVariable("self").AccessExpression;
				}
				case "host":
				{
					if (preScrFn == null)
						throw new LsnrParsingException(token, "Cannot use 'host' outside a script object method or event listener.", script.Path);
					return new HostInterfaceAccessExpression(((IBasePreScriptClass)preScrFn.Parent).HostId);
				}
				case "none": return LsnValue.Nil;
				default:
					throw new LsnrParsingException(token, $"Cannot parse token '{token.Value}' as an expression.", script.Path);
			}
		}

		/// <summary>
		/// Creates an array of arrays of tokens from the given <see cref="Token"/>s.
		/// Also returns the index of the next token after the procedure call (i.e. the token after the closing parenthesis).
		/// </summary>
		/// <param name="indexOfOpen">The index of the opening parenthesis.</param>
		/// <param name="tokens">The tokens.</param>
		/// <param name="script">The script.</param>
		/// <returns></returns>
		/// <exception cref="LsnrParsingException">...</exception>
		public static (Token[][] argTokens, int indexOfNextToken) CreateArgList(int indexOfOpen, IReadOnlyList<Token> tokens, IPreScript script)
		{
			var argTokens = new List<Token[]>();
			var currentList = new List<Token>();
			var j = indexOfOpen;
			if (j >= tokens.Count || tokens[j].Value != "(")
				throw new LsnrParsingException(tokens[j], "...", script.Path);

			void Pop()
			{
				argTokens.Add(currentList.ToArray());
				currentList.Clear();
			}

			var balance = 1;
			while (balance != 0)
			{
				++j;
				var t = tokens[j];
				if (t.Value == "(")
				{
					++balance;
					currentList.Add(t);
				}
				else if (t.Value == ")")
				{
					--balance;
					if (balance != 0)
						currentList.Add(t);
				}
				else if (t.Value == ",")
				{
					if (balance == 1)
						Pop();
					else
						currentList.Add(t);
				}
				else currentList.Add(t);
			}
			if (currentList.Count != 0)
				Pop();

			return (argTokens.ToArray(), j + 1);
		}

		/// <summary>
		/// Creates <see cref="IExpression"/>s for the arguments of a procedure call where there are no named arguments.
		/// Also returns the index of the next token after the procedure call (i.e. the token after the closing parenthesis).
		/// </summary>
		/// <param name="indexOfOpen">The index of open.</param>
		/// <param name="tokens">The tokens.</param>
		/// <param name="script">The script.</param>
		/// <param name="substitutions">The substitutions.</param>
		/// <returns></returns>
		public static (IExpression[] args, int nextIndex)
			CreateArgs(int indexOfOpen, IReadOnlyList<Token> tokens, IPreScript script, IReadOnlyDictionary<Token, IExpression> substitutions = null)
		{
			var (argTokens, indexOfNextToken) = CreateArgList(indexOfOpen, tokens, script);
			var args = new IExpression[argTokens.Length];
			for (var i = 0; i < argTokens.Length; i++)
				args[i] = ExpressionParser.Parse(argTokens[i], script, substitutions);
			return (args, indexOfNextToken);
		}
	}
}
