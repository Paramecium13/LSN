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
		public OpCode OpCode { get; }

		public ushort Data { get; }

		/// <inheritdoc />
		public InstructionMappedMethod(LsnType type, LsnType returnType, string name, OpCode opCode,
			IReadOnlyList<Parameter> parameters = null, ushort data = 0) : this(type.Id, returnType.Id, name, opCode,
			parameters, data) {}

		/// <inheritdoc />
		public InstructionMappedMethod(TypeId type, TypeId returnType, string name, OpCode opCode,
			IReadOnlyList<Parameter> parameters = null, ushort data = 0)
			: base(type, returnType, name, parameters ?? new List<Parameter> {new Parameter("self", type, LsnValue.Nil, 0)})
		{
			OpCode = opCode;
			Data = data;
		}
	}

	public sealed class InstructionMappedFunction : Function
	{
		public OpCode OpCode { get; }

		public ushort Data { get; }

		public InstructionMappedFunction(IReadOnlyList<Parameter> parameters, LsnType returnType, string name,
			OpCode opCode, ushort data = 0)
			: this(parameters, returnType?.Id, name, opCode, data)
		{}
		
		public InstructionMappedFunction(IReadOnlyList<Parameter> parameters, TypeId returnType, string name,
			OpCode opCode, ushort data = 0)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			OpCode = opCode;
			Data = data;
		}
	}

	public sealed class MultiInstructionMappedFunction : Function
	{
		public (OpCode Opcode, ushort Data)[][] Instructions { get; }

		/// <inheritdoc />
		public MultiInstructionMappedFunction(IReadOnlyList<Parameter> parameters, LsnType returnType, string name,
			(OpCode opCode, ushort data)[][] instructionSets)
			: base(new FunctionSignature(parameters, name, returnType?.Id))
		{
			if (instructionSets.Length != parameters.Count) throw new ArgumentException("Bad format...", nameof(instructionSets));
			Instructions = instructionSets;
		}
	}
}
