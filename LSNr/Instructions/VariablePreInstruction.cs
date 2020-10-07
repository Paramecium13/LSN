using System;
using LsnCore;

namespace LSNr
{
	/// <summary>
	/// A <see cref="PreInstruction"/> that does something with a <see cref="LSNr.Variable"/>
	/// </summary>
	/// <seealso cref="LSNr.PreInstruction" />
	public abstract class VariablePreInstruction : PreInstruction
	{
		protected VariablePreInstruction(Variable variable, OpCode code) : base(code)
		{
			Variable = variable;
		}

		/// <summary>
		/// Gets the <see cref="LSNr.Variable"/> that this instruction works with.
		/// </summary>
		public Variable Variable { get; }
	}

	/// <summary>
	/// A <see cref="PreInstruction"/> that loads a variable or constant.
	/// </summary>
	/// <seealso cref="LSNr.VariablePreInstruction" />
	public sealed class LoadVariablePreInstruction : VariablePreInstruction
	{
		/// <summary>
		/// The data
		/// </summary>
		private ushort _data;

		/// <inheritdoc />
		public override ushort Data => _data;

		/// <inheritdoc />
		public LoadVariablePreInstruction(Variable variable) : base(variable, OpCode.LoadLocal)
		{ }

		/// <inheritdoc />
		/// <remarks>
		/// Resolves whether or not the variable is a constant and if it is, generates the instruction to load it.
		/// </remarks>
		public override unsafe void Resolve()
		{
			base.Resolve();
			if (Variable.Const())
			{
				var value = (LsnValue)Variable.AccessExpression;
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
						var sh = (short)intVal;
						_data = *(ushort*)&sh;
						return;
					}
					throw new NotImplementedException();
				}

				throw new NotImplementedException();
			}

			_data = (ushort)Variable.Index;
		}
	}

	/// <summary>
	/// A <see cref="PreInstruction"/> that loads the value of a variable onto the evaluation stack.
	/// </summary>
	/// <seealso cref="LSNr.VariablePreInstruction" />
	public sealed class SetVariablePreInstruction : VariablePreInstruction
	{
		/// <summary>
		/// The data
		/// </summary>
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

}