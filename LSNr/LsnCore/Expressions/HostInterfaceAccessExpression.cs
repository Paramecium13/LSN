using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	/// <summary>
	/// An expression that gets the host of the current script class.
	/// </summary>
	/// <seealso cref="LsnCore.Expressions.IExpression" />
	public sealed class HostInterfaceAccessExpression : IExpression
	{
		/// <inheritdoc/>
		public bool IsPure => false;

		/// <inheritdoc/>
		public TypeId Type { get; }

		public HostInterfaceAccessExpression(TypeId type)
		{
			Type = type;
		}

		/// <inheritdoc/>
		public bool Equals(IExpression other)
		{
			return other is HostInterfaceAccessExpression;
		}

		/// <inheritdoc />
		public IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			instructions.AddInstruction(new SimplePreInstruction(OpCode.GetHost, 0));
		}

#if CORE
		public LsnValue Eval(IInterpreter i)
		{
			return (i.GetVariable(0).Value as ScriptObject).GetHost();
		}
#endif

		/// <inheritdoc/>
		public IExpression Fold() => this;

		/// <inheritdoc/>
		public bool IsReifyTimeConst() => false;

		/// <inheritdoc/>
		public void Replace(IExpression oldExpr, IExpression newExpr) {}// _ScriptObject should be a variable access expression, accessing the 'self' parameter of
																		// a script object method.

		/// <inheritdoc/>
		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.HostInterfaceAccess);
		}

		/// <inheritdoc/>
		public IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
