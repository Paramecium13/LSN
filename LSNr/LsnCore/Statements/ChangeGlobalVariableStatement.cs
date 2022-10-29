using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using LSNr.CodeGeneration;
using LSNr;

namespace LsnCore.Statements
{
	public sealed class ChangeGlobalVariableStatement : Statement
	{
		private readonly string GlobalVarName;

		private IExpression Value;

		public ChangeGlobalVariableStatement(string name, IExpression value)
		{
			GlobalVarName = name; Value = value;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			//i.SetGlobalVariable(Value.Eval(i), GlobalVarName);
			throw new NotImplementedException();
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value == oldExpr) Value = newExpr;
		}

		internal override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}

		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}
	}
}
