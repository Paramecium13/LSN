using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.Converations;
using LSNr.ScriptObjects;

namespace LSNr.ReaderRules
{
	public interface IFunctionContainer : ITypeContainer
	{
		string Path { get; }
		bool Valid { get; set; }
		Function GetFunction(string name);

		SymbolType CheckSymbol(string symbol);
		IReadOnlyList<Parameter> ParseParameters(IReadOnlyList<Token> tokens);
		IProcedure CreateFunction(IReadOnlyList<Parameter> args, TypeId retType, string name, bool isVirtual = false);
	}

	public interface IPreResource : IFunctionContainer
	{
		IPreScript Script { get; }

		void RegisterUsing(string file);

		void RegisterFunction(Function fn);

		void RegisterTypeId(TypeId id);
		void RegisterStructType(StructType structType);
		void RegisterRecordType(RecordType recordType);
		void RegisterHandleType(HandleType handleType);
		void RegisterHostInterface(HostInterfaceType host);
		void RegisterScriptClass(ScriptClass scriptClass);

		event Action<IPreResource> ParseSignaturesA;
		event Action<IPreResource> ParseSignaturesB;
		event Action<IPreResource> ParseProcBodies;

		LsnResourceThing Parse();
	}

	public static class TypeContainerExtensions
	{
		/// <summary>
		/// ...
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="self"></param>
		/// <param name="index">The starting index.</param>
		/// <returns>The parameters</returns>
		internal static IReadOnlyList<Parameter> BaseParseParameters(this IFunctionContainer self, IReadOnlyList<Token> tokens, ushort index = 0)
		{
			var paramaters = new List<Parameter>();
			for (int i = 0; i < tokens.Count; i++)
			{
				var name = tokens[i].Value;
				if (tokens[++i].Value != ":")
					throw new LsnrParsingException(tokens[i], $"Expected token ':' after parameter name {name} received token '{tokens[i].Value}'.", self.Path);
				var type = self.ParseTypeId(tokens, ++i, out i);
				var defaultValue = LsnValue.Nil;
				if (i < tokens.Count && tokens[i].Value == "=")
				{
					if (tokens[++i].Type == TokenType.String)
					{
						if (type != LsnType.string_.Id)
							throw new LsnrParsingException(tokens[i], $"Error in parsing parameter {name}: cannot assign a default value of type string to a parameter of type {type.Name}", self.Path);
						defaultValue = new LsnValue(new StringValue(tokens[i].Value));
						if (i + 1 < tokens.Count) i++;
					}
					else if (tokens[i].Type == TokenType.Integer)
					{
						if (type != LsnType.int_.Id)
						{
							if (type == LsnType.double_.Id)
							{
								defaultValue = new LsnValue(tokens[i].IntValue);
							}
							else
								throw new LsnrParsingException(tokens[i], $"Error in parsing parameter {name}: cannot assign a default value of type int to a parameter of type {type.Name}", self.Path);
						}
						else defaultValue = new LsnValue(tokens[i].IntValue);
						if (i + 1 < tokens.Count) i++;
					}
					else if (tokens[i].Type == TokenType.Float)
					{
						if (type != LsnType.double_.Id)
							throw new LsnrParsingException(tokens[i], $"Error in parsing parameter {name}: cannot assign a default value of type double to a parameter of type {type.Name}", self.Path);
						defaultValue = new LsnValue(tokens[i].DoubleValue);
						if (i + 1 < tokens.Count) i++;
					}
					// Bools and other stuff...
					else throw new LsnrParsingException(tokens[i], $"Error in parsing default value for parameter {name}.", self.Path);
				}
				paramaters.Add(new Parameter(name, type, defaultValue, index++));
				if (i < tokens.Count && tokens[i].Value != ",")
					throw new LsnrParsingException(tokens[i], $"expected token ',' after definition of parameter {name}, received '{tokens[i].Value}'.", self.Path);
			}
			return paramaters;
		}

		internal static void TestToken(this ISlice<Token> self, int index, string expectedVal, string file, string expectedMsg = null,
			StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
			=> self.TestToken(index, (s) => s.Equals(expectedVal, stringComparison), expectedMsg ?? expectedVal, file);

		internal static void TestToken(this ISlice<Token> self, int index, Predicate<string> test, string expectedMsg, string file)
		{
			if (!test(self[index].Value))
				throw LsnrParsingException.UnexpectedToken(self[index], expectedMsg, file);
		}
	}

	public abstract class ResourceReaderStatementRule : IReaderStatementRule
	{
		protected readonly IPreResource PreResource;

		protected ResourceReaderStatementRule(IPreResource pre) { PreResource = pre; }

		public abstract bool Check(ISlice<Token> tokens);

		/// <summary>
		/// The terminal semicolon is included.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="attributes"></param>
		public abstract void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes);
	}

	sealed class ResourceUsingStatementRule : ResourceReaderStatementRule
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

	sealed class ResourceHandleTypeStatementRule : ResourceReaderStatementRule
	{
		public ResourceHandleTypeStatementRule(IPreResource pre) : base(pre) { }

		public override void Apply(ISlice<Token> tokens, ISlice<Token>[] attributes)
		{
			if (tokens[1].Type != TokenType.Identifier)
				throw new LsnrParsingException(tokens[1], "Invalid handle name", PreResource.Path);
			var name = tokens[1].Value;
			if (PreResource.TypeExists(name))
				throw new LsnrParsingException(tokens[1], "Type already has name", PreResource.Path);

			var handle = new HandleType(name, out TypeId id);

			PreResource.RegisterTypeId(id);

			var parents = new List<Token>();

			if(tokens.Length > 3)
			{
				tokens.TestToken(2, ":", PreResource.Path, ": or ;");
				if (tokens.Length == 4)
					throw new LsnrParsingException(tokens[3], "Expected list of parent handle types.", PreResource.Path);
				var lastWasComma = false;
				for (int i = 3; i < tokens.Length - 1; i++)
				{
					var token = tokens[i];
					switch (token.Type)
					{
						case TokenType.Identifier:
							parents.Add(token);
							lastWasComma = false;
							break;
						case TokenType.SyntaxSymbol:
							if(lastWasComma)
								throw LsnrParsingException.UnexpectedToken(token,"the name of a handle type", PreResource.Path);
							if (token.Value != ",")
								throw LsnrParsingException.UnexpectedToken(token,lastWasComma ? "the name of a handle type" : "a comma or the name of a handle type", PreResource.Path);
							lastWasComma = true;
							break;
						default:
							throw LsnrParsingException.UnexpectedToken(token,lastWasComma ? "the name of a handle type" : "a comma or the name of a handle type", PreResource.Path);
					}
				}
			}

			var builder = new HandleBuilder(handle,parents.ToArray());
			PreResource.RegisterHandleType(handle);
			PreResource.ParseSignaturesA += builder.OnParsingSignatures;
		}

		public override bool Check(ISlice<Token> tokens)
			=> tokens.Length >= 3 && tokens[0].Value == "handle";
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
						if (i >= head.Count)
							throw new LsnrParsingException(fnToken, "error parsing return type.", PreResource.Path);
						count++;
					}
					returnType = head.CreateSubSlice(start, count);
				}
			}
			if (head[i].Value != "{")
				throw LsnrParsingException.UnexpectedToken(head[i], "{", PreResource.Path);
			var fn = new FunctionBuilder(paramTokens.ToSlice(), returnType, body, name);
			PreResource.ParseSignaturesA += fn.OnParsingSignatures;
			PreResource.ParseProcBodies += fn.OnParsingProcBodies;
		}
	}

	sealed class ResourceReaderStructRule : ResourceReaderBodyRule
	{
		internal ResourceReaderStructRule(IPreResource pre) : base(pre) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Value == "struct";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			if (head.Count != 2)
				throw new LsnrParsingException(head[0], "Invalid struct...", PreResource.Path);
			var id = new TypeId(head[1].Value);
			PreResource.RegisterTypeId(id);
			var builder = new StructBuilder(id, body);
			PreResource.ParseSignaturesA += builder.OnParsingSignatures;
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
			PreResource.ParseSignaturesA += builder.OnParsingSignatures;
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
			var id = new TypeId(name);
			PreResource.RegisterTypeId(id);
			var host = new HostInterfaceComponent(PreResource, id, body, PreResource.Script.Path);
			PreResource.ParseSignaturesA += host.OnParsingSignatures;
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
			var scriptClass = new ScriptClassBuilder(PreResource, name, host, unique, meta, body);
				//new PreScriptClass(name, PreResource.Script, host, unique, meta, body);
			PreResource.ParseSignaturesB += scriptClass.OnParsingSignaturesB;
			PreResource.ParseProcBodies  += scriptClass.OnParsingProcBodies;
			PreResource.RegisterTypeId(scriptClass.Id);
		}
	}

	sealed class ResourceReaderConversationRule : ResourceReaderBodyRule
	{
		public ResourceReaderConversationRule(IPreResource p) : base(p) { }

		public override bool Check(ISlice<Token> head)
			=> head[0].Type == TokenType.Keyword && head[0].Value == "conversation";

		public override void Apply(ISlice<Token> head, ISlice<Token> body, ISlice<Token>[] attributes)
		{
			if (head.Count < 3 || head[1].Type != TokenType.Identifier)
				throw new LsnrParsingException(head[0], "Improperly formatted conversation.", PreResource.Path);
			var name = head[1].Value;
			ISlice<Token> args = null;
			if (head[2].Value == "(")
			{
				var i = new Indexer<Token>(3, head);
				args = i.SliceWhile(t => t.Value != ")", out bool err);
				if (err)
					throw new LsnrParsingException(head[0], $"Error parsing parameter list of conversation '{name}'.", PreResource.Path);
			}
			var conv = new ConversationBuilder(PreResource, name, args);
			var reader = new ConversationReader(conv, body);
			reader.Read();
			PreResource.ParseSignaturesA += conv.OnParsingSignatures;
			PreResource.ParseProcBodies += (_) => conv.Parse();
		}
	}
}
