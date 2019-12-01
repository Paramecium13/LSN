using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ReaderRules;

namespace LSNr.ScriptObjects
{
	public interface IPreHostInterface : IFunctionContainer
	{
		void RegisterEvent(EventDefinition ev);
		void RegisterMethod(FunctionSignature fn);
	}

	public abstract class HostInterfaceStatementRule : IReaderStatementRule
	{
		protected readonly IPreHostInterface PreHostInterface;
		protected HostInterfaceStatementRule(IPreHostInterface pre) { PreHostInterface = pre; }

		public abstract void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> tokens);
	}

	public abstract class HostInterfaceBodyRule : IReaderBodyRule
	{
		protected readonly IPreHostInterface PreHostInterface;
		protected HostInterfaceBodyRule(IPreHostInterface pre) { PreHostInterface = pre; }

		public abstract void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
		public abstract bool Check(ISlice<Token> head);
	}

	public sealed class HostInterfaceReader : RuledReader<HostInterfaceStatementRule, HostInterfaceBodyRule>
	{
		protected override IEnumerable<HostInterfaceStatementRule> StatementRules { get; }

		protected override IEnumerable<HostInterfaceBodyRule> BodyRules { get; } = new HostInterfaceBodyRule[0];

		internal HostInterfaceReader(ISlice<Token> tokens, IPreHostInterface pre) : base(tokens)
		{
			StatementRules = new HostInterfaceStatementRule[]
			{
				new HostInterfaceMethodRule(pre),
				new HostInterfaceEventRule(pre)
			};
		}

		public void Read() { ReadTokens(); }

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes) {}
	}

	public sealed class HostInterfaceMethodRule : HostInterfaceStatementRule
	{
		public HostInterfaceMethodRule(IPreHostInterface pre) : base(pre) { }

		public override bool Check(ISlice<Token> tokens) => tokens[0].Value == "fn";

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			var i = 1;
			var name = tokens[i].Value;
			i++; // 'i' should now point to the opening parenthesis.
			if (tokens[i].Value != "(")
				throw new LsnrParsingException(tokens[i], $"Error parsing method definition '{name}': Expected '('.", PreHostInterface.Path);

			var paramTokens = new List<Token>();
			while (tokens[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(tokens[i]);

			var parameters = PreHostInterface.ParseParameters(paramTokens);
			TypeId returnType = null;
			i++; // 'i' Points to the thing after the closing parenthesis.
			if (tokens.TestAt(i, t => t.Value == "->"))
			{
				i++; // 'i' points to the thing after '->'.
				if (tokens[i].Value == "(")
				{
					if (tokens[++i].Value != ")")
						throw new LsnrParsingException(tokens[i], $"Error parsing method definition '{name}': Expected '('.", PreHostInterface.Path);
					// 'i' points to ')'.
					i++; // 'i' points to the thing after ')'.
				}
				else
				{
					var j = i;
					returnType = PreHostInterface.ParseTypeId(tokens, i, out i); // i points to the thing after the return type.
					if (i < 0)
						throw new LsnrParsingException(tokens[j], $"Error parsing method definition '{name}': Failed to parse return type.", PreHostInterface.Path);
				}
			}
			if (i > tokens.Count)
				throw new LsnrParsingException(tokens[i - 1], $"Error parsing method definition '{name}': Expected ';' or '->'.", PreHostInterface.Path);
			PreHostInterface.RegisterMethod(new FunctionSignature(parameters, name, returnType));
		}
	}

	public sealed class HostInterfaceEventRule : HostInterfaceStatementRule
	{
		public HostInterfaceEventRule(IPreHostInterface pre) : base(pre) { }

		public override bool Check(ISlice<Token> tokens) => tokens[0].Value == "event";

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			var i = 1;
			var name = tokens[i].Value;
			i++; // 'i' should now point to the opening parenthesis.
			if (tokens[i].Value != "(")
				throw new LsnrParsingException(tokens[i - 1], $"Error parsing event definition '{name}': Expected '('.", PreHostInterface.Path);

			var paramTokens = new List<Token>();
			while (tokens[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(tokens[i]);

			var parameters = PreHostInterface.ParseParameters(paramTokens);

			i++; // 'i' Points to the thing after the closing parenthesis.
			if (i > tokens.Count)
				throw new LsnrParsingException(tokens[i >= tokens.Count ? i - 1 : i], $"Error parsing event definition '{name}': Expected ';'.", PreHostInterface.Path);

			PreHostInterface.RegisterEvent(new EventDefinition(name, parameters));
		}
	}
}
