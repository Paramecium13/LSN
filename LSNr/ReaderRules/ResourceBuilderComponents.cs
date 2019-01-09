using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using System;
using System.Collections.Generic;

namespace LSNr.ReaderRules
{
	public abstract class SimpleTypeBuilder
	{
		protected readonly TypeId Id;
		protected readonly ISlice<Token> Tokens;
		protected SimpleTypeBuilder(TypeId id, ISlice<Token> tokens)
		{
			Id = id; Tokens = tokens;
		}

		protected static Tuple<string, TypeId>[] ParseFields(ITypeContainer typeContainer, ISlice<Token> tokens, string path)
		{
			if (tokens.Length < 3) // struct Circle { Radius : double}
			{
				throw new LsnrParsingException(tokens[0], "too few tokens.", path);
			}
			var fields = new List<Tuple<string, TypeId>>();
			for (int i = 0; i < tokens.Length; i++)
			{
				var fName = tokens[i++].Value; // Get the name of the field, move on to the next token.
				if (i >= tokens.Length) // Make sure the definition does not end..
					throw new LsnrParsingException(tokens[i - 1], "unexpected end of declaration, expected ':'.", path);
				if (tokens[i++].Value != ":") // Make sure the next token is ':', move on to the next token.
					throw LsnrParsingException.UnexpectedToken(tokens[i - 1], ":", path);
				if (i >= tokens.Length) // Make sure the definition does not end.
					throw new LsnrParsingException(tokens[i - 1], "unexpected end of declaration, expected type.", path);
				var tId = typeContainer.ParseTypeId(tokens, i, out i);
				fields.Add(new Tuple<string, TypeId>(fName, tId));
				if (i + 1 < tokens.Length && tokens[i].Value == ",") // Check if the definition ends, move on to the next token
																	 // and check that it is ','.
				{
					// Move on to the next token, which should be the name of the next field.
					continue; // Move on to the next field.
				}
				break;
			}
			return fields.ToArray();
		}
		public abstract void OnParsingSignatures(IPreResource preResource);
	}

	public class StructBuilder : SimpleTypeBuilder
	{
		public StructBuilder(TypeId id, ISlice<Token> tokens):base(id,tokens) {}

		public override void OnParsingSignatures(IPreResource preResource)
		{
			var fields = ParseFields(preResource, Tokens, preResource.Path);
			var structType = new StructType(Id, fields);
			preResource.RegisterStructType(structType);
		}
	}

	public class RecordBuilder : SimpleTypeBuilder
	{
		public RecordBuilder(TypeId id, ISlice<Token> tokens) : base(id, tokens) { }

		public override void OnParsingSignatures(IPreResource preResource)
		{
			var fields = ParseFields(preResource, Tokens, preResource.Path);
			var recordType = new RecordType(Id, fields);
			preResource.RegisterRecordType(recordType);
		}
	}
}
