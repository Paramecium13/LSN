using System;
using System.Collections.Generic;
using System.Linq;
using LsnCore.Expressions;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public class AssignmentStatement : Statement
	{
		public IExpression Value;

		public Variable Variable { get; }

		public AssignmentStatement(Variable variable, IExpression value)
		{
			Variable = variable;
			Value = value;
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			i.SetVariable(Index, Value.Eval(i));
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value.Equals(oldExpr)) Value = newExpr;
		}

		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			Value.GetInstructions(instructionList, context.WithContext(ExpressionContext.Store));
			instructionList.AddInstruction(new SetVariablePreInstruction(Variable));
		}

		protected virtual IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{}
	}
}
