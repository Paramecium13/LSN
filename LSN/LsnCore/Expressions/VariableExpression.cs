using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{

	[Serializable]
	public class VariableExpression : Expression
	{
		public int Index;

		public override bool IsPure => true;

		public VariableExpression(int index, TypeId type)
		{
			Index = index; Type = type;
		}

		public VariableExpression(int index)
		{
			Index = index;
		}


		public override LsnValue Eval(IInterpreter i)
			=> i.GetVariable(Index);
		

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.Variable);
			writer.Write((ushort)Index);
		}
	}
}
