using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

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

		public override InterpretValue Interpret(IInterpreter i)
		{
			//i.SetGlobalVariable(Value.Eval(i), GlobalVarName);
			throw new NotImplementedException();
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value == oldExpr) Value = newExpr;
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}
	}
}
