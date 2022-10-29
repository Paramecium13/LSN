using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Values;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	/// <summary>
	/// A statement that sets the state of the script object that the current procedure belongs to (Local Variable 0).
	/// </summary>
	/// <seealso cref="LsnCore.Statements.Statement" />
	public sealed class SetStateStatement : Statement
	{
		private readonly int State;

		/// <summary>
		/// Initializes a new instance of the <see cref="SetStateStatement"/> class.
		/// </summary>
		/// <param name="state">The state to set the script object to.</param>
		public SetStateStatement(int state)
		{
			State = state;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			(i.GetVariable(0).Value as ScriptObject).SetState(State);
			return InterpretValue.Base;
		}
#endif

		/// <inheritdoc />
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}

		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.SetState);
			writer.Write(State);
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			instructionList.AddInstruction(new SimplePreInstruction(OpCode.SetState, (ushort) State));
		}
	}
}
