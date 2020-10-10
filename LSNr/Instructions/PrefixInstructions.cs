using LsnCore;

namespace LSNr
{
	/// <summary>
	/// A <see cref="LSNr.BasePreInstruction"/> for prefix <see cref="OpCode"/>s, such as <see cref="OpCode.Line"/>.
	/// </summary>
	/// <seealso cref="LSNr.BasePreInstruction" />
	public abstract class PrePrefixInstruction : BasePreInstruction
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PrePrefixInstruction"/> class.
		/// </summary>
		/// <param name="code">The <see cref="OpCode"/>.</param>
		protected PrePrefixInstruction(OpCode code) : base(code){}
	}

	/// <summary>
	/// A <see cref="BasePreInstruction"/> that notes the line number in source code of the following instructions.
	/// </summary>
	/// <seealso cref="LSNr.PrePrefixInstruction" />
	public sealed class LineNumberPreInstruction : PrePrefixInstruction
	{
		/// <inheritdoc/>
		public override ushort Data { get; }

		/// <inheritdoc />
		public LineNumberPreInstruction(int lineNumber) : base(OpCode.Line)
		{
			Data = (ushort)lineNumber;
		}
	}

	public sealed class LoadTempPreInstruction : PrePrefixInstruction
	{
		/// <inheritdoc />
		public LoadTempPreInstruction(ushort data) : base(OpCode.LoadTempIndex)
		{
			Data = data;
		}

		/// <inheritdoc />
		public override ushort Data { get; }
	}
}