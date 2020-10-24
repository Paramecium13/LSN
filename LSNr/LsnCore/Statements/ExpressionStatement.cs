﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public sealed class ExpressionStatement : Statement
	{
		private IExpression Expression; // I may have to expose this for optimization.

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

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Expression.Equals(oldExpr))
				Expression = newExpr;
			else
				Expression.Replace(oldExpr, newExpr);
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.EvaluateExpression);
			Expression.Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc/>
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
		}
	}
}
