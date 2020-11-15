using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Types;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	/// <summary>
	/// A statement that consists of an <see cref="IExpression"/> that is executed.
	/// </summary>
	/// <seealso cref="LsnCore.Statements.Statement" />
	public sealed class ExpressionStatement : Statement
	{
		private IExpression Expression; // I may have to expose this for optimization.

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionStatement"/> class.
		/// </summary>
		/// <param name="expression">The expression.</param>
		public ExpressionStatement(IExpression expression)
		{
			Expression = expression;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			Expression.Eval(i);
			return InterpretValue.Base;
		}
#endif

		/// <inheritdoc />
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Expression.Equals(oldExpr))
				Expression = newExpr;
			else
				Expression.Replace(oldExpr, newExpr);
		}

		/// <inheritdoc />
		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.EvaluateExpression);
			Expression.Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc/>
		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Expression;
			foreach (var expr in Expression.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			Expression.GetInstructions(instructionList, context.WithContext(ExpressionContext.SubExpression));
			if (Expression.Type == null) return;
			if (Expression.Type.Type is StructType strType)
			{
				instructionList.AddInstruction(new SimplePreInstruction(OpCode.FreeStruct,
					(ushort) strType.FieldCount));
			}
			else
			{
				instructionList.AddInstruction(new SimplePreInstruction(OpCode.Pop, 0));
			}
		}
	}
}
