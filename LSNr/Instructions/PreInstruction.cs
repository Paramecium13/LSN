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
		/// <param name="resolutionContext"></param>
		public virtual void Resolve(InstructionResolutionContext resolutionContext) {}
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

		/// <param name="resolutionContext"></param>
		/// <inheritdoc/>
		public override void Resolve(InstructionResolutionContext resolutionContext)
		{
			base.Resolve(resolutionContext);
			PrefixInstructions.ForEach(i => i.Resolve(resolutionContext));
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

		private ushort _Data;

		/// <inheritdoc />
		public override ushort Data => _Data;// Function.Index;

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionCallPreInstruction"/> class.
		/// </summary>
		/// <param name="function">The function that the instruction will call.</param>
		public FunctionCallPreInstruction(Function function) : base(OpCode.HCF)
		{
			Function = function;
		}

		/// <param name="resolutionContext"></param>
		/// <inheritdoc />
		public override void Resolve(InstructionResolutionContext resolutionContext)
		{
			base.Resolve(resolutionContext);
			if (Function.ResourceFilePath == resolutionContext.FileHeaderFactory.FilePath)
			{
				Code = OpCode.CallFn_Local;
				_Data = resolutionContext.GetLocalProcedureIndex(Function);
				return;
			}

			resolutionContext.FileHeaderFactory.AddFunctionReferenceByName(Function.ResourceFilePath, Function.Name,
				out var referencedFileIndex, out var fnIndex);
			if (fnIndex <= byte.MaxValue && referencedFileIndex <= byte.MaxValue)
			{
				var data = (fnIndex << 8) | referencedFileIndex;
				_Data = checked((ushort) data);
				Code = OpCode.CallFn_Short;
				return;
			}

			PrefixInstructions.Add(new LoadTempPreInstruction(referencedFileIndex));
			Code = OpCode.CallFn;
			_Data = fnIndex;
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

		/// <summary>
		/// Gets the name of the host interface the method was defined in.
		/// </summary>
		internal string HostInterfaceName { get;}

		/// <summary>
		/// The data
		/// </summary>
		private ushort _Data;

		/// <inheritdoc />
		public override ushort Data => _Data;

		/// <summary>
		/// Initializes a new instance of the <see cref="HostInterfaceMethodCallPreInstruction"/> class.
		/// </summary>
		/// <param name="methodSignature">The signature of the host interface method.</param>
		public HostInterfaceMethodCallPreInstruction(FunctionSignature methodSignature)
			: base(methodSignature.ReturnType != null? OpCode.CallHostInterfaceMethod : OpCode.CallHostInterfaceMethodVoid)
		{
			MethodSignature = methodSignature;
			HostInterfaceName = methodSignature.Parameters[0].Type.Name;
		}

		/// <param name="resolutionContext"></param>
		/// <inheritdoc/>
		public override void Resolve(InstructionResolutionContext resolutionContext)
		{
			base.Resolve(resolutionContext);
			_Data = resolutionContext.FileHeaderFactory.AddHostInterfaceMethodSignature(
				new LsnCore.Interpretation.SignatureStub(MethodSignature, HostInterfaceName));
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public sealed class InstructionResolutionContext
	{
		/// <summary>
		/// Gets the file header factory.
		/// </summary>
		public ObjectFileHeaderFactory FileHeaderFactory { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="InstructionResolutionContext"/> class.
		/// </summary>
		/// <param name="fileHeaderFactory">The file header factory.</param>
		public InstructionResolutionContext(ObjectFileHeaderFactory fileHeaderFactory)
		{
			FileHeaderFactory = fileHeaderFactory;
		}

		/// <summary>
		/// Gets the index of the locally defined procedure.
		/// </summary>
		/// <param name="function">The function.</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public ushort GetLocalProcedureIndex(Function function) => throw new NotImplementedException();
	}
}
