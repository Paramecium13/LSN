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

		bool Valid { get; }

		void RegisterUsing(string file);

		void RegisterScriptClass(string name, string hostname, bool unique, string metadata, ISlice<Token> tokens);
		void RegisterHostInterface(string name, ISlice<Token> tokens);
		List<Parameter> ParseParameters(IReadOnlyList<Token> tokens);

		void RegisterFunction(string name, ISlice<Token> args, ISlice<Token> returnType, ISlice<Token> body);

		void RegisterScriptClass(ScriptClass scriptClass);
		void RegisterStructType(StructType structType);
		void RegisterRecordType(RecordType recordType);
		void RegisterTypeId(TypeId id);

		event Action<IPreResource> ParseSignatures;
		event Action<IPreResource> ParseProcBodies;

		LsnResourceThing Parse();
	}

	public abstract class ResourceReaderStatementRule : IReaderStatementRule
	{
		protected readonly IPreResource PreResource;

		protected ResourceReaderStatementRule(IPreResource pre) { PreResource = pre; }

		public abstract bool Check(ISlice<Token> tokens);
		public abstract void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
	}

	class ResourceUsingStatementRule : ResourceReaderStatementRule
	{
		readonly DependencyWaiter Waiter;
		public ResourceUsingStatementRule(IPreResource pre, DependencyWaiter waiter) : base(pre)
		{ Waiter = waiter; }

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			if (tokens.Count > 3 || tokens[1].Type != TokenType.String)
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
		public abstract void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes);
	}

	sealed class ResourceReaderFunctionRule : ResourceReaderBodyRule
	{
		public ResourceReaderFunctionRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "fn";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
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
			// At this point, the current token (i.e. tokens[i].Value) is ')'.
			ISlice<Token> returnType = null;
			if (head[++i].Value == "->")
			{
				i++; // The current token is the token after '->'.
				if (head[i].Value == "(")
				{
					if (head[++i].Value != ")")
						throw LsnrParsingException.UnexpectedToken(head[i], ")", PreResource.Path);
				}
				else
				{
					var start = i++; var count = 1;
					while (head[i].Value != "{")
					{
						i++;
						if (i >= head.Count) throw new LsnrParsingException(fnToken, "error parsing return type.", PreResource.Path);
						count++;
					}
					returnType = head.CreateSubSlice(start, count);
				}
			}
			if (head[i].Value != "{")
				throw LsnrParsingException.UnexpectedToken(head[i], "{", PreResource.Path);

			PreResource.RegisterFunction(name,paramTokens.ToSlice(),returnType, body);
		}
	}

	sealed class ResourceReaderStructRule : ResourceReaderBodyRule
	{
		internal ResourceReaderStructRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "struct";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			if (head.Count > 3 || head.Count < 2)
				throw new LsnrParsingException(head[0], "Invalid struct...", PreResource.Path);
			var id = new TypeId(head[1].Value);
			PreResource.RegisterTypeId(id);
			var builder = new StructBuilder(id, body);
			PreResource.ParseSignatures += builder.OnParsingSignatures;
		}
	}

	sealed class ResourceReaderRecordRule : ResourceReaderBodyRule
	{
		internal ResourceReaderRecordRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "record";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			if (head.Count > 3 || head.Count < 2)
				throw new LsnrParsingException(head[0], "Invalid record...", PreResource.Path);
			var id = new TypeId(head[1].Value);
			PreResource.RegisterTypeId(id);
			var builder = new RecordBuilder(id, body);
			PreResource.ParseSignatures += builder.OnParsingSignatures;
		}
	}

	sealed class ResourceReaderHostInterfaceRule : ResourceReaderBodyRule
	{
		internal ResourceReaderHostInterfaceRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "hostinterface" || (head[0].Value == "host" && head.Count > 1 && head[1].Value == "interface");

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
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

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			var i = 0;
			var unique = attributes.Any(a => a.Count != 0 && a[0].Value == "unique");
			string host = null;
			string meta = null;
			var m = attributes.FirstOrDefault(a => a.Count != 0 && string.Equals(a[0].Value, "metadata", StringComparison.OrdinalIgnoreCase));
			if(m != null)
			{
				// [metadata: "stuff"]
				if (m.Count != 3 || m[1].Value != ":" || m[2].Type != TokenType.String)
					throw new LsnrParsingException(m[0], "Improperly formatted metadata...", PreResource.Path);
				meta = m[2].Value;
			}
			/*if (head[i].Value == "unique")
			{
				unique = true;
				i++;
			}*/
			if (head[i].Value == "scriptclass") i++;
			else i += 2;
			if (i >= head.Count) throw new LsnrParsingException(head[head.Count - 1], "...", PreResource.Path);
			var name = head[i++].Value;
			if (i < head.Count && head[i].Value == "[")
			{							// name		[	meta	]	 <
										// -1		i	+1		+2	 +3
				if (++i >= head.Count)	// -2		-1	 i		+1	 +2
					throw new LsnrParsingException(head[head.Count - 1], "", PreResource.Path);
				if (head[i].Type != TokenType.String)
					throw LsnrParsingException.UnexpectedToken(head[i], "a string literal", PreResource.Path);
				meta = head[i].Value;
				if (++i >= head.Count)  //	-3		-2	 -1		i	 +1
					throw new LsnrParsingException(head[i - 1], "...", PreResource.Path);
				if (head[i].Value != "]") throw LsnrParsingException.UnexpectedToken(head[i], "]", PreResource.Path);
				++i;					//	-4		-3	 -2		-1	  i
			}
			if (i + 1 < head.Count)
			{
				if (head[i].Value != "<") throw LsnrParsingException.UnexpectedToken(head[i], "<", PreResource.Path);
				if (++i >= head.Count) throw new LsnrParsingException(head[i - 1], "...", PreResource.Path);
				host = head[i].Value;
				if (++i == head.Count) throw new LsnrParsingException(head[i - 1], "...", PreResource.Path);
				if (head[i].Value != "{")
					throw LsnrParsingException.UnexpectedToken(head[i], "{", PreResource.Path);
			}
			PreResource.RegisterScriptClass(name, host, unique, meta, body);
		}
	}
}
