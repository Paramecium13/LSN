using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;

namespace LSNr.ReaderRules
{
	public interface IPreResource : ITypeContainer
	{
		string Path { get; }

		void RegisterUsing(string file);

		void RegisterStructType(string name, ISlice<Token> tokens);
		void RegisterRecordType(string name, ISlice<Token> tokens);
		void RegisterScriptClass(string name, string hostname, bool unique, string metadata, ISlice<Token> tokens);
		void RegisterHostInterface(string name, ISlice<Token> tokens);
		List<Parameter> ParseParameters(IReadOnlyList<Token> tokens);

		void RegisterFunction(LsnFunction function, ISlice<Token> body);
	}

	public abstract class ResourceReaderStatementRule : IReaderStatementRule
	{
		protected readonly IPreResource PreResource;

		protected ResourceReaderStatementRule(IPreResource pre) { PreResource = pre; }

		public abstract bool Check(ISlice<Token> tokens);
		public abstract void Apply(ISlice<Token> tokens);
	}

	class ResourceUsingStatementRule : ResourceReaderStatementRule
	{
		public ResourceUsingStatementRule(IPreResource pre) : base(pre) { }

		public override void Apply(ISlice<Token> tokens)
		{
			if (tokens.Count != 2 || tokens[0].Type != TokenType.String)
				throw new LsnrParsingException(tokens[0], "Invalid \'using\' statement.", PreResource.Path);
			PreResource.RegisterUsing(tokens[1].Value);
		}

		public override bool Check(ISlice<Token> tokens)
			=> tokens[0].Value == "using";
	}

	public abstract class ResourceReaderBodyRule : IReaderBodyRule
	{
		protected readonly IPreResource PreResource;

		protected ResourceReaderBodyRule(IPreResource pre) { PreResource = pre; }

		public abstract bool Check(ISlice<Token> head);
		public abstract void Apply(ISlice<Token> head, ISlice<Token> body);
	}

	sealed class ResourceReaderFunctionRule : ResourceReaderBodyRule
	{
		public ResourceReaderFunctionRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "fn";

		public override void Apply(ISlice<Token> head, ISlice<Token> body)
		{
			var i = 0;
			var fnToken = head[i];
			var name = head[++i].Value;
			//TODO : validate name.
			if (head[++i].Value != "(")
				throw LsnrParsingException.UnexpectedToken(head[i], "(", PreResource.Path);
			var paramTokens = new List<Token>();
			while (head[++i].Value != ")") // This starts with the token after '('.
				paramTokens.Add(head[i]);
			var paramaters = PreResource.ParseParameters(paramTokens);
			// At this point, the current token (i.e. tokens[i].Value) is ')'.
			TypeId returnType = null;
			if (head[++i].Value == "->")
			{
				if (head[++i].Value == "(")
				{ // The current token is the token after '->'.
					if (head[++i].Value != ")")
						throw LsnrParsingException.UnexpectedToken(head[i], ")", PreResource.Path);
				}
				else
				{ // The current token is the token after '->'.
					try
					{
						returnType = PreResource.ParseTypeId(head, i, out i);
					}
					catch (Exception e)
					{
						throw new LsnrParsingException(fnToken, "error parsing return type.", e, PreResource.Path);
					}
					if (i < 0) throw new LsnrParsingException(fnToken, "error parsing return type.", PreResource.Path);
				}
			}
			if (head[i].Value != "{")
				throw LsnrParsingException.UnexpectedToken(head[i], "}", PreResource.Path);

			var fn = new LsnFunction(paramaters, returnType, name, PreResource.Path);
			throw new NotImplementedException();
		}
	}

	sealed class ResourceReaderStructRule : ResourceReaderBodyRule
	{
		internal ResourceReaderStructRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "struct";

		public override void Apply(ISlice<Token> head, ISlice<Token> body)
		{
			if (head.Count != 2)
				throw new LsnrParsingException(head[0], "Invalid struct...", PreResource.Path);
			PreResource.RegisterStructType(head[1].Value, body);
		}
	}

	sealed class ResourceReaderRecordRule : ResourceReaderBodyRule
	{
		internal ResourceReaderRecordRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "record";

		public override void Apply(ISlice<Token> head, ISlice<Token> body)
		{
			if (head.Count != 2)
				throw new LsnrParsingException(head[0], "Invalid record...", PreResource.Path);
			PreResource.RegisterRecordType(head[1].Value, body);
		}
	}

	sealed class ResourceReaderHostInterfaceRule : ResourceReaderBodyRule
	{
		internal ResourceReaderHostInterfaceRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "hostinterface" || (head[0].Value == "host" && head.Count > 1 && head[1].Value == "interface");

		public override void Apply(ISlice<Token> head, ISlice<Token> body)
		{
			if (head.Count < 2)
				throw new LsnrParsingException(head[0], "Invalid Host Interface.", PreResource.Path);
			string name;
			if (head[0].Value == "host") name = head[2].Value;
			else name = head[1].Value;
			PreResource.RegisterHostInterface(name, body);
		}
	}

	sealed class ResourceReaderScriptClassRule : ResourceReaderBodyRule
	{
		internal ResourceReaderScriptClassRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "scriptclass" || (head[0].Value == "script" && head.Count > 1 && head[1].Value == "class") ||
				(head[0].Value == "unique" && head.Count > 1 && ((head[1].Value == "scriptclass") || (head.Count > 2 && head[2].Value == "class")));

		public override void Apply(ISlice<Token> head, ISlice<Token> body)
		{
			throw new NotImplementedException();
		}
	}
}
