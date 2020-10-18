using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;

namespace LsnCore
{
	public sealed class InstructionMappedMethod : Method
	{
		/// <summary>
		/// The <see cref="LsnCore.OpCode"/> for the <see cref="Instruction"/> that implements this method.
		/// </summary>
		public OpCode OpCode { get; }

		/// <summary>
		/// The data for the <see cref="Instruction"/> that implements this method.
		/// </summary>
		public ushort Data { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="InstructionMappedMethod"/> class.
		/// </summary>
		/// <param name="type">The type this method belongs to.</param>
		/// <param name="returnType">The method's return type.</param>
		/// <param name="name">The name.</param>
		/// <param name="opCode">The <see cref="LsnCore.OpCode"/>.</param>
		/// <param name="parameters">The method's parameters.</param>
		/// <param name="data">The data for this method's <see cref="Instruction"/>.</param>
		public InstructionMappedMethod(LsnType type, LsnType returnType, string name, OpCode opCode,
			IReadOnlyList<Parameter> parameters = null, ushort data = 0) : this(type.Id, returnType.Id, name, opCode,
			parameters, data) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="InstructionMappedMethod"/> class.
		/// </summary>
		/// <param name="type">The type this method belongs to.</param>
		/// <param name="returnType">The method's return type.</param>
		/// <param name="name">The name.</param>
		/// <param name="opCode">The <see cref="LsnCore.OpCode"/>.</param>
		/// <param name="parameters">The method's parameters.</param>
		/// <param name="data">The data for this method's <see cref="Instruction"/>.</param>
		public InstructionMappedMethod(TypeId type, TypeId returnType, string name, OpCode opCode,
			IReadOnlyList<Parameter> parameters = null, ushort data = 0)
			: base(type, returnType, name, parameters ?? new List<Parameter> {new Parameter("self", type, LsnValue.Nil, 0)})
		{
			OpCode = opCode;
			Data = data;
		}
	}

	/// <summary>
	/// A <see cref="Function"/> that is implemented as a single <see cref="Instruction"/>.
	/// </summary>
	/// <seealso cref="LsnCore.Function" />
	public sealed class InstructionMappedFunction : Function
	{
		/// <summary>
		/// The <see cref="LsnCore.OpCode"/> for the instruction.
		/// </summary>
		public OpCode OpCode { get; }

		/// <summary>
		/// The data for the instruction.
		/// </summary>
		public ushort Data { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="InstructionMappedFunction"/> class.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		/// <param name="returnType">The function's return type.</param>
		/// <param name="name">The name of the function.</param>
		/// <param name="opCode">The <see cref="LsnCore.OpCode"/> for the instruction.</param>
		/// <param name="data">The data for the instruction.</param>
		public InstructionMappedFunction(IReadOnlyList<Parameter> parameters, LsnType returnType, string name,
			OpCode opCode, ushort data = 0)
			: this(parameters, returnType?.Id, name, opCode, data)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="InstructionMappedFunction"/> class.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		/// <param name="returnType">The function's return type.</param>
		/// <param name="name">The name of the function.</param>
		/// <param name="opCode">The <see cref="LsnCore.OpCode"/> for the instruction.</param>
		/// <param name="data">The data for the instruction.</param>
		public InstructionMappedFunction(IReadOnlyList<Parameter> parameters, TypeId returnType, string name,
			OpCode opCode, ushort data = 0)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			OpCode = opCode;
			Data = data;
		}
	}

	/// <summary>
	/// A <see cref="Function"/> that is implemented by multiple instructions.
	/// </summary>
	/// <seealso cref="LsnCore.Function" />
	public sealed class MultiInstructionMappedFunction : Function
	{
		/// <summary>
		/// Gets the <see cref="OpCode"/>s and data for the instructions.
		/// The instructions are split into a number of sets equal to the number of parameters that this function takes.
		/// <para/>
		/// Each set of instructions is inserted after the instructions for the corresponding parameter.
		/// </summary>
		public (OpCode Opcode, ushort Data)[][] Instructions { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiInstructionMappedFunction"/> class.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <param name="name">The name.</param>
		/// <param name="instructionSets">
		///		The <see cref="OpCode"/>s and data for the instructions.
		///		The instructions are split into a number of sets equal to the number of parameters that this function takes.
		///		<para/>
		///		Each set of instructions is inserted after the instructions for the corresponding parameter.
		/// </param>
		/// <exception cref="ArgumentException">Bad format... - instructionSets</exception>
		public MultiInstructionMappedFunction(IReadOnlyList<Parameter> parameters, LsnType returnType, string name,
			(OpCode opCode, ushort data)[][] instructionSets)
			: base(new FunctionSignature(parameters, name, returnType?.Id))
		{
			if (instructionSets.Length != parameters.Count) throw new ArgumentException("Bad format...", nameof(instructionSets));
			Instructions = instructionSets;
		}
	}
}
