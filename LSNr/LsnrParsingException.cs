using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	[Serializable]
	internal class LsnrParsingException : LsnrException
	{
		public Token Token { get; private set; }

		public int LineNumber => Token.LineNumber;

		public override string Message => $"Error line {LineNumber}: {base.Message}";

		/*public LsnrParsingException() { }
		public LsnrParsingException(string message) : base(message) { }
		public LsnrParsingException(string message, Exception inner) : base(message, inner) { }
		protected LsnrParsingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

		protected LsnrParsingException(string message, string file) : base(message, file){}
		protected LsnrParsingException(string message, Exception inner, string file) : base(message, inner, file){}*/

		internal LsnrParsingException(Token token, string message, string file) : base(message, file)
		{
			Token = token;
		}

		internal LsnrParsingException(Token token, string message, Exception inner, string file) : base(message, inner, file)
		{
			Token = token;
		}

		internal static LsnrParsingException UnexpectedToken(Token givenToken, string expectedToken, string file)
			=> new LsnrParsingException(givenToken, $"Expected \'{expectedToken}\', received {givenToken.Value}", file);

		internal static LsnrParsingException TypeMismatch(Token token, string expectedType, string providedType, string file)
			=> new LsnrParsingException(token, $"Expected an expression of type '{expectedType}', received an expression of type '{providedType}'.", file);
	}
}
