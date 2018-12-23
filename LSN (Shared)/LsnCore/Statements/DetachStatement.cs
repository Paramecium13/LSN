using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Values;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public class DetachStatement : Statement
	{
		public override InterpretValue Interpret(IInterpreter i)
		{
			(i.GetVariable(0).Value as ScriptObject).Detach();
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.Detach);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}
	}
}
