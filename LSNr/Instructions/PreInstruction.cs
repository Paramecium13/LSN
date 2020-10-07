using LsnCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;

namespace LSNr
{
	/// <summary>
	/// A base pre instruction. It is used to generate an <see cref="Instruction"/>.
	/// </summary>
	public abstract class BasePreInstruction
	{
		/// <summary>
		/// The data portion of this instruction.
		/// </summary>
		public abstract ushort Data { get; }

		/// <summary>
		/// The <see cref="OpCode"/> for this instruction.
		/// Subtypes may need to change the value of this based on later conditions.
		/// <para/>
		/// For example, a function call instruction may choose between <see cref="OpCode.CallFn"/>,
		/// <see cref="OpCode.CallFn_Local"/>, and <see cref="OpCode.CallFn_Short"/>. based on if the function
		/// is local and if its index is short.
		/// </summary>
		public OpCode Code { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BasePreInstruction"/> class.
		/// </summary>
		/// <param name="code">The <see cref="OpCode"/>.</param>
		protected BasePreInstruction(OpCode code)
		{
			Code = code;
		}

		/// <summary>
		/// Resolves the details of this instruction. For example, choose between long and short forms of the instruction,
		/// in the former case, requires a load instruction.
		/// </summary>
		public virtual void Resolve() {}
	}

	/// <summary>
	/// A pre-Instruction. It is used to generate main instructions (i.e. ones that aren't line number instructions or similar not really executed instructions).
	/// </summary>
	/// <seealso cref="LSNr.BasePreInstruction" />
	public abstract class PreInstruction : BasePreInstruction
	{
		// ReSharper disable once UnusedMember.Global
		/// <summary>
		/// The labels applied to this instruction.
		/// </summary>
		internal List<InstructionLabel> Labels { get; } = new List<InstructionLabel>();

		/// <summary>
		/// The prefix instructions.
		/// </summary>
		internal List<PrePrefixInstruction> PrefixInstructions { get; } = new List<PrePrefixInstruction>();

		/// <summary>
		/// Gets the size of this pre instruction, including any prefix instructions, in <see cref="Instruction"/>s.
		/// </summary>
		internal int Size => PrefixInstructions.Count + 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="PreInstruction"/> class.
		/// </summary>
		/// <param name="code">The <see cref="OpCode" />.</param>
		protected PreInstruction(OpCode code) : base(code) { }

		/// <inheritdoc/>
		public override void Resolve()
		{
			base.Resolve();
			PrefixInstructions.ForEach(i => i.Resolve());
		}
	}

	/// <summary>
	/// A simple <see cref="PreInstruction"/> that just contains its <see cref="BasePreInstruction.Code"/> and <see cref="BasePreInstruction.Data"/>.
	/// It does not have any information that needs to be resolved.
	/// </summary>
	/// <seealso cref="LSNr.PreInstruction" />
	public sealed class SimplePreInstruction : PreInstruction
	{
		/// <inheritdoc />
		public override ushort Data { get; }

		/// <inheritdoc />
		public SimplePreInstruction(OpCode code, ushort data) : base(code)
		{
			Data = data;
		}
	}

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
			Data = (ushort) lineNumber;
		}
	}

	/// <summary>
	/// An instruction that has a target, such as <see cref="OpCode.Jump"/> or <see cref="OpCode.RegisterChoice"/>. 
	/// </summary>
	/// <seealso cref="PreInstruction" />
	public sealed class TargetedPreInstruction : PreInstruction
	{
		/// <summary>
		/// Gets the <see cref="Instruction"/> that this <see cref="TargetedPreInstruction"/> targets.
		/// </summary>
		public InstructionLabel Target { get; }

		/// <inheritdoc/>
		public override ushort Data => Target.Index;

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetedPreInstruction"/> class.
		/// </summary>
		/// <param name="code">The <see cref="OpCode"/> of this instruction.</param>
		/// <param name="label">The name of the label that this instruction targets..</param>
		/// <param name="factory">The label factory.</param>
		public TargetedPreInstruction(OpCode code, string label, InstructionLabel.Factory factory)
			:this(code,factory.GetLabel(label)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetedPreInstruction"/> class.
		/// </summary>
		/// <param name="code">The <see cref="OpCode"/> of the instruction.</param>
		/// <param name="label">The <see cref="InstructionLabel"/> that the instruction targets.</param>
		public TargetedPreInstruction(OpCode code, InstructionLabel label):base(code)
		{
			Target = label;
		}
	}

	/// <summary>
	/// A <see cref="PreInstruction"/> that refers to a <see cref="LsnType"/>
	/// </summary>
	/// <seealso cref="LSNr.PreInstruction" />
	public class TypeTargetedInstruction : PreInstruction
	{
		/// <summary>
		/// Gets the <see cref="TypeId"/> that this instruction refers to.
		/// </summary>
		public TypeId TypeId { get; }

		/// <inheritdoc />
		// ToDo: Create property TypeId.Index
		public override ushort Data => throw new NotImplementedException(); //TypeId.Index;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeTargetedInstruction"/> class.
		/// </summary>
		/// <param name="code">The <see cref="OpCode"/>.</param>
		/// <param name="type">The <see cref="TypeId"/> that the instruction targets.</param>
		public TypeTargetedInstruction(OpCode code, TypeId type) : base(code)
		{
			TypeId = type;
		}
	}

	/// <summary>
	/// A <see cref="PreInstruction"/> for function calls.
	/// </summary>
	/// <seealso cref="LSNr.PreInstruction" />
	public class FunctionCallPreInstruction : PreInstruction
	{
		/// <summary>
		/// Gets the function.
		/// </summary>
		internal Function Function { get; }

		/// <inheritdoc />
		public override ushort Data => throw new NotImplementedException();// Function.Index;

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionCallPreInstruction"/> class.
		/// </summary>
		/// <param name="function">The function that the instruction will call.</param>
		public FunctionCallPreInstruction(Function function) : base(OpCode.HCF)
		{
			Function = function;
		}

		/// <inheritdoc />
		public override void Resolve()
		{
			base.Resolve();
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// A <see cref="PreInstruction"/> that calls a host interface method.
	/// </summary>
	/// <seealso cref="LSNr.PreInstruction" />
	public class HostInterfaceMethodCallPreInstruction : PreInstruction
	{
		/// <summary>
		/// Gets the signature of the host interface method to call.
		/// </summary>
		internal FunctionSignature MethodSignature { get; }

		/// <inheritdoc />
		public override ushort Data => throw new NotImplementedException();

		/// <summary>
		/// Initializes a new instance of the <see cref="HostInterfaceMethodCallPreInstruction"/> class.
		/// </summary>
		/// <param name="methodSignature">The signature of the host interface method.</param>
		public HostInterfaceMethodCallPreInstruction(FunctionSignature methodSignature)
			: base(methodSignature.ReturnType != null? OpCode.CallHostInterfaceMethod : OpCode.CallHostInterfaceMethodVoid)
		{
			MethodSignature = methodSignature;
		}

		/// <inheritdoc/>
		public override void Resolve()
		{
			base.Resolve();
			throw new NotImplementedException();
		}
	}

	// ReSharper disable once UnusedMember.Global	

}
