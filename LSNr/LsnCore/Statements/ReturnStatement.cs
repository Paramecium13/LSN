using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;
using MoreLinq;

namespace LsnCore.Statements
{
	/// <summary>
	/// Returns from a function, possibly with a value
	/// </summary>
	public class ReturnStatement : Statement
	{
		/// <summary>
		/// Gets or sets the value that this <see cref="ReturnStatement"/> returns.
		/// </summary>
		public IExpression Value { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ReturnStatement"/> class.
		/// </summary>
		/// <param name="e">The expression to return, or null if it doesn't return a value.</param>
		public ReturnStatement(IExpression e)
		{
			Value = e;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			if(Value != null) i.ReturnValue = Value.Eval(i);
			return InterpretValue.Return;
		}
#endif

		/// <inheritdoc />
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value == null) return;
			if (oldExpr.Equals(Value))
				Value = newExpr;
			else
				Value.Replace(oldExpr, newExpr);
		}

		/// <inheritdoc />
		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			if(Value == null)
				writer.Write(StatementCode.Return);
			else
			{
				writer.Write(StatementCode.ReturnValue);
				Value.Serialize(writer, resourceSerializer);
			}
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			if (!(!Value?.Equals(LsnValue.Nil) ?? false)) yield break;
			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			Value?.GetInstructions(instructionList, context.WithContext(ExpressionContext.ReturnValue));
			instructionList.AddInstruction(new SimplePreInstruction(OpCode.Ret, 0));
		}
	}
}
