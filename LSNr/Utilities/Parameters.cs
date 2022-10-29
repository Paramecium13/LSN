using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.LssParser;

namespace LSNr.Utilities
{
	static class Parameters
	{
		public static string GetSignature(this IReadOnlyList<Parameter> self)
		{
			var str = new StringBuilder();
			void Parse(Parameter param)
			{
				str.Append(param.Name)
					.Append(": ")
					.Append(param.Type);
				if (param.DefaultValue.IsNull) return;
				// default value;
				str.Append(" = ");
				if (param.Type == LsnType.Bool_.Id)
					str.Append(param.DefaultValue.BoolValue.ToString());
				else if (param.Type == LsnType.double_.Id)
					str.Append(param.DefaultValue.DoubleValue.ToString());
				else if (param.Type == LsnType.int_.Id)
					str.Append(param.DefaultValue.IntValue.ToString());
				else if (param.Type == LsnType.string_.Id)
					str.Append('"')
						.Append((param.DefaultValue.Value as StringValue).Value);
				else
					str	.Append("<<DEFAULT = ")
						.Append(param.DefaultValue.Value != null ? param.DefaultValue.Value.ToString() : param.DefaultValue.RawData.ToString("X"))
						.Append(" >>");
			}
			for (var i = 0; i < self.Count - 1; i++)
			{
				Parse(self[i]);
				str.Append(", ");
			}
			Parse(self[self.Count - 1]);

			return str.ToString();
		}

		public static IExpression[] Check(Token startToken, IReadOnlyList<Parameter> parameters, IExpression[] args, IPreScript script, string procName)
		{
			if(parameters.Count != args.Length)
			{
				if (parameters.Count > args.Length)
				{
					var count = args.Length;
					var old = args;
					args = new IExpression[parameters.Count];
					Array.Copy(old, args, old.Length);
					for (int i = count; i < args.Length; i++)
					{
						if (parameters[i].DefaultValue.IsNull)
							throw new LsnrParsingException(startToken, $"{parameters[i].Name} of {procName} does not have a default value.", script.Path);
						args[i] = parameters[i].DefaultValue;
					}
				}
				else
					throw new LsnrParsingException(startToken, $"Passed to {procName}.", script.Path);
			}
			for (int i = 0; i < args.Length; i++)
			{
				if (!parameters[i].Type.Subsumes(args[i].Type))
					throw new LsnrParsingException(startToken,
						$"{parameters[i].Name} of {procName} has a type of {parameters[i].Type.Name}; it cannot be passed a value of {args[i].Type.Name}", script.Path);
			}

			return args;
		}

		public static (ISlice<Token>[] argTokens, int indexOfNextToken) CreateArgList(int indexOfOpen, IReadOnlyList<Token> tok, IPreScript script)
		{
			var tokens = tok.ToSlice();
			var argTokens = new List<ISlice<Token>>();

			var currentStart = indexOfOpen + 1;
			var currentCount = 0;
			var j = indexOfOpen;

			if (j >= tokens.Count || tokens[j].Value != "(")
				throw new LsnrParsingException(tokens[j], "...", script.Path);

			void pop()
			{
				var current = tokens.CreateSliceSkipTake(currentStart, currentCount);
				currentCount = 0;
				currentStart = j + 1;
				argTokens.Add(current);
			}

			var balance = 1;
			while (balance != 0)
			{
				++j;
				var t = tokens[j];
				if (t.Value == "(")
				{
					++balance;
					++currentCount;
				}
				else if (t.Value == ")")
				{
					--balance;
					if (balance != 0)
						++currentCount;
				}
				else if (t.Value == ",")
				{
					if (balance == 1)
						pop();
					else
						++currentCount;
				}
				else ++currentCount;
			}
			if (currentCount != 0)
				pop();

			return (argTokens.ToArray(), j + 1);
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="indexOfOpen"></param>
		/// <param name="tokens"></param>
		/// <param name="procTitle"></param>
		/// <param name="parameters"></param>
		/// <param name="script"></param>
		/// <param name="substitutions"></param>
		/// <param name="self">todo: describe self parameter on CreateArgs</param>
		/// <returns></returns>
		public static (IExpression[] args, int nextIndex)
			CreateArgs(int indexOfOpen, IReadOnlyList<Token> tokens, string procTitle, IReadOnlyList<Parameter> parameters, IPreScript script, IExpression self = null,
			IReadOnlyDictionary<Token, IExpression> substitutions = null)
		{
			var isMember = self != null;

			var (argTokens, indexOfNextToken) = CreateArgList(indexOfOpen, tokens, script);

			var argExprs = new IExpression[parameters.Count];

			if (isMember) argExprs[0] = self;

			var exprIndex = isMember ? 1 : 0;
			foreach (var arg in argTokens)
			{
				IExpression expr;
				if (arg.TestAt(1, (t) => t.Value == ":"))
				{
					var arg1 = arg;
					exprIndex = parameters.IndexOf(p => p.Name == arg1[0].Value);
					if (exprIndex < 0)
						throw new LsnrParsingException(arg[0], $"{procTitle} does not have a parameter named {arg[0].Value}", script.Path);
					expr = ExpressionParser.Parse(arg.CreateSliceAt(2), script, substitutions);
				}
				else
				{
					if(exprIndex < argExprs.Length)
						expr = Create.Express(arg, script, substitutions);
					else
					{
						exprIndex = argExprs.IndexOf(null);
						if (exprIndex < 0)
							throw new LsnrParsingException(arg[0], $"{procTitle} only has {parameters.Count.ToString()} parameters.", script.Path);
						expr = Create.Express(arg, script, substitutions);
					}
				}

				if (isMember && exprIndex == 0)
					throw new LsnrParsingException(arg[0], "Cannot provide a value for the parameter 'self'.", script.Path);
				var param = parameters[exprIndex];
				if (argExprs[exprIndex] != null)
					throw new LsnrParsingException(arg[0], $"The parameter {param.Name} of {procTitle} has already been given a value.", script.Path);
				// Type Check:
				if (!param.Type.Subsumes(expr.Type))
					throw new LsnrParsingException(arg[0], $"{param.Name} of {procTitle} has a type of {param.Type.Name}; it cannot be passed a value of {expr.Type.Name}", script.Path);

				argExprs[exprIndex++] = expr;
			}

			// default values.

			return (argExprs, indexOfNextToken);
		}

		// Parse parameters (replace Create.CreateArgs())
	}
}
