using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	[Serializable]
	public abstract class LsnrException : Exception
	{
		/// <summary>
		/// The LSN source file responsible for this exception.
		/// </summary>
		public string File { get; protected set; }
		protected LsnrException() { }
		protected LsnrException(string message) : base(message){}

		protected LsnrException(string message, Exception inner) : base(message, inner){}

		protected LsnrException(string message, string file) : base(message)
		{
			File = file;
		}

		protected LsnrException(string message, Exception inner, string file) : base(message, inner)
		{
			File = file;
		}

		protected LsnrException(System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context){}
	}

	[Serializable]
	internal class LsnrConstructException : LsnrException
	{
		public override string Message => base.Message + "\n\t" + InnerException.Message.Replace("\n", "\n\t");

		public LsnrConstructException(string message, LsnrException inner) : base(message, inner, inner.File) { }
	}
}
