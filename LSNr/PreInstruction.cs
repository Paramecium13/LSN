using LsnCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	// ReSharper disable once UnusedMember.Global
	public abstract class PreInstruction
	{
		public OpCode OpCode { get; }

		public abstract ushort Data { get; }

		// ReSharper disable once UnusedMember.Global
		internal List<InstructionLabel> Labels { get; } = new List<InstructionLabel>();

		protected PreInstruction(OpCode opCode)
		{
			OpCode = opCode;
		}
	}

	public sealed class SimplePreInstruction : PreInstruction
	{
		/// <inheritdoc />
		public override ushort Data { get; }

		/// <inheritdoc />
		public SimplePreInstruction(OpCode opCode, ushort data) : base(opCode)
		{
			Data = data;
		}

	}

	// ReSharper disable once UnusedMember.Global
	internal class InstructionLabel : IEquatable<InstructionLabel>
	{
		/// <summary>
		/// The name (optional).
		/// </summary>
		public string Name { get; }
		
		public InstructionLabel(string name)
		{
			Name = name;
		}

		/// <inheritdoc />
		public bool Equals(InstructionLabel other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Name == other.Name;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((InstructionLabel) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}
