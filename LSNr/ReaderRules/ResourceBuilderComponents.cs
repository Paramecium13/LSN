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

	public class HandleBuilder
	{
		readonly HandleType Type;
		readonly Token[] Parents;
		public HandleBuilder(HandleType type, Token[] parents)
		{
			Type = type; Parents = parents;
		}

		public void OnParsingSignatures(IPreResource preResource)
		{
			foreach (var parent in Parents)
			{
				if (!preResource.TypeExists(parent.Value))
					throw new LsnrParsingException(parent, $"The type '{parent.Value}' does not exist.", preResource.Path);
				try
				{
					var ty = preResource.GetType(parent.Value);
					var hty = (HandleType)ty;
					Type.AddParent(hty);
				}
				catch (InvalidCastException)
				{ throw new LsnrParsingException(parent, $"The type '{parent.Value}' is not a handle type.", preResource.Path); }
				catch (ApplicationException e)
				{ throw new LsnrParsingException(parent, e.Message, preResource.Path); }
				catch (Exception e)
				{ throw new LsnrParsingException(parent, $"Error parsing handle type '{Type.Name}'.", e, preResource.Path); }
			}
		}
	}

	public class FunctionBuilder
	{
		readonly string Name;
		readonly ISlice<Token> Args;
		readonly ISlice<Token> ReturnType;
		readonly ISlice<Token> Body;

		LsnFunction Function;

		public FunctionBuilder(ISlice<Token> args, ISlice<Token> ret, ISlice<Token> body, string name)
		{ Args = args; ReturnType = ret; Body = body; Name = name; }

		public void OnParsingSignatures(IPreResource resource)
		{
			TypeId ret = null;
			if (ReturnType != null)
				ret = resource.ParseTypeId(ReturnType, 0, out int i);
			Function = new LsnFunction(resource.ParseParameters(Args),ret, Name, resource.Path);
			resource.RegisterFunction(Function);
		}

		public void OnParsingProcBodies(IPreResource resource)
		{
			try
			{
				var preFn = new PreFunction(resource.Script);
				foreach (var param in Function.Parameters)
					preFn.CurrentScope.CreateVariable(param);
				var cg = new CodeGen(preFn, Function.ReturnType, $"function '{Function.Name}'");
				cg.Generate(Body);
				if (preFn.Valid)
				{
					Function.Code = cg.Code;
					Function.StackSize = cg.StackSize;
				}
				else
					resource.Valid = false;
			}
			catch (LsnrException e)
			{
				Logging.Log("function", Function.Name, e);
				resource.Valid = false;
			}
			catch (Exception e)
			{
				Logging.Log("function", Function.Name, e, resource.Path);
				resource.Valid = false;
			}
		}
	}
}
