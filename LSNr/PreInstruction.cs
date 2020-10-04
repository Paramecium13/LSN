using LsnCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;

namespace LSNr
{
	// ReSharper disable once UnusedMember.Global
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
		/// Gets the size of this pre instruction, including any prefix instructions.
		/// </summary>
		internal int Size => PrefixInstructions.Count + 1;

		/// <inheritdoc />
		protected PreInstruction(OpCode code) : base(code) { }

		/// <inheritdoc/>
		public override void Resolve()
		{
			base.Resolve();
			PrefixInstructions.ForEach(i => i.Resolve());
		}
	}

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

	public abstract class PrePrefixInstruction : BasePreInstruction
	{
		protected PrePrefixInstruction(OpCode code) : base(code){}
	}

	public sealed class LineNumberPreInstruction : PrePrefixInstruction
	{
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
		public InstructionLabel Target { get; }

		public override ushort Data => Target.Index;

		public TargetedPreInstruction(OpCode code, string label, InstructionLabel.Factory factory)
			:this(code,factory.GetLabel(label)) { }

		public TargetedPreInstruction(OpCode code, InstructionLabel label):base(code)
		{
			Target = label;
		}
	}

	public class TypeTargetedInstruction : PreInstruction
	{
		public TypeId TypeId { get; }

		/// <inheritdoc />
		// ToDo: Create property TypeId.Index
		public override ushort Data => throw new NotImplementedException(); //TypeId.Index;

		public TypeTargetedInstruction(OpCode code, TypeId type) : base(code)
		{
			TypeId = type;
		}
	}

	public class FunctionCallPreInstruction : PreInstruction
	{
		/// <summary>
		/// Gets the function.
		/// </summary>
		internal Function Function { get; }

		/// <inheritdoc />
		public override ushort Data => throw new NotImplementedException();// Function.Index;

		/// <inheritdoc />
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

	public class HostInterfaceMethodCallPreInstruction : PreInstruction
	{
		internal FunctionSignature MethodSignature { get; }

		/// <inheritdoc />
		public override ushort Data => throw new NotImplementedException();

		/// <inheritdoc />
		public HostInterfaceMethodCallPreInstruction(FunctionSignature methodSignature) : base(OpCode.CallHostInterfaceMethodVoid)
		{
			MethodSignature = methodSignature;
		}

		public override void Resolve()
		{
			base.Resolve();
			throw new NotImplementedException();
		}
	}

	public abstract class VariablePreInstruction : PreInstruction
	{
		protected VariablePreInstruction(Variable variable, OpCode code) : base(code)
		{
			Variable = variable;
		}

		public Variable Variable { get; }
	}

	public sealed class LoadVariablePreInstruction : VariablePreInstruction
	{
		private ushort _data;

		/// <inheritdoc />
		public override ushort Data => _data;

		/// <inheritdoc />
		public LoadVariablePreInstruction(Variable variable) : base(variable, OpCode.LoadLocal)
		{}

		/// <inheritdoc />
		public override unsafe void Resolve()
		{
			base.Resolve();
			if (Variable.Const())
			{
				var value = (LsnValue) Variable.AccessExpression;
				if (Variable.Type == LsnType.Bool_)
				{
					Code = OpCode.LoadConst_I32_short;
					_data = value.BoolValueSimple ? (ushort)0 : (ushort)1;
					return;
				}

				if (Variable.Type == LsnType.int_)
				{
					var intVal = value.IntValue;
					if (intVal >= short.MinValue && intVal <= short.MaxValue)
					{
						Code = OpCode.LoadConst_I32_short;
						var sh = (short) intVal;
						_data = *(ushort*)&sh;
						return;
					}
				}
			}

			_data = (ushort) Variable.Index;
		}
	}

	public sealed class SetVariablePreInstruction : VariablePreInstruction
	{
		private ushort _data;

		/// <inheritdoc />
		public override ushort Data => _data;

		/// <summary>
		/// Initializes a new instance of the <see cref="SetVariablePreInstruction"/> class.
		/// </summary>
		/// <param name="variable">The variable.</param>
		public SetVariablePreInstruction(Variable variable) : base(variable, OpCode.StoreLocal) { }

		/// <inheritdoc />
		public override void Resolve()
		{
			base.Resolve();
			_data = (ushort)Variable.Index;
		}
	}

	// ReSharper disable once UnusedMember.Global	
	/// <summary>
	/// A label for <see cref="PreInstruction"/>s. Its <see cref="Index"/> will be resolved when all instructions are ready...
	/// </summary>
	public class InstructionLabel
	{
		/// <summary>
		/// The index of the instruction this label points to.
		/// </summary>
		public ushort Index { get; set; }

		/// <summary>
		/// The name (optional).
		/// </summary>
		public string Name { get; }
		
		private InstructionLabel(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Creates <see cref="InstructionLabel"/>s...
		/// </summary>
		public class Factory
		{
			private readonly Dictionary<string, InstructionLabel> LabelsLookup =
				new Dictionary<string, InstructionLabel>();

			// ReSharper disable once UnusedMember.Global
			public IEnumerable<InstructionLabel> Labels => LabelsLookup.Values;

			// ReSharper disable once EmptyConstructor
			public Factory() { }

			/// <summary>
			/// Gets the label with the given <paramref name="name"/> if it already exists, otherwise creates it.
			/// </summary>
			/// <param name="name">The name.</param>
			/// <returns></returns>
			public InstructionLabel GetLabel(string name)
			{
				if (LabelsLookup.TryGetValue(name, out var label))
				{
					return label;
				}

				label = new InstructionLabel(name);
				LabelsLookup.Add(name, label);
				return label;
			}
		}
	}

	public enum ExpressionContext
	{
		/// <summary>
		/// A sub expression
		/// </summary>
		SubExpression,
		/// <summary>
		/// A field of this expression's return value is being written to.
		/// </summary>
		FieldWrite,
		/// <summary>
		/// For an array or list: this expression's result will have an item stored in it.
		/// </summary>
		ItemWrite,
		/// <summary>
		/// This expression's return value is being stored somewhere.
		/// <para/>
		/// It is up to the expression given this context to emit the copy struct instruction when needed.
		/// <para/>
		/// For example, variable access and index access instructions need to copy the struct but procedure calls and constructors don't
		/// </summary>
		Store,
		/// <summary>
		/// This expression's return value is being passed as a parameter--normally (i.e. structs will be copied).
		/// <para/>
		/// In this case, the procedure call expression will emit the copy struct instruction when needed.
		/// </summary>
		Parameter_Default,
	}

	public class InstructionGenerationContext
	{
		public InstructionLabel.Factory LabelFactory { get; }

		public ExpressionContext Context { get; }

		public InstructionGenerationContext(ExpressionContext context, InstructionLabel.Factory labelFactory)
		{
			Context = context;
			LabelFactory = labelFactory;
		}

		/// <summary>
		/// Creates a copy of this with a different context.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public InstructionGenerationContext WithContext(ExpressionContext context) =>
			new InstructionGenerationContext(context, LabelFactory);
	}
}
