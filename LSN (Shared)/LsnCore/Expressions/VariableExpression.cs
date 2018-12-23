using System.Collections.Generic;
using LsnCore.Types;
#if LSNR
using LSNr;
#endif
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class VariableExpression : Expression
	{
		public int Index;

		public override bool IsPure => true;

#if LSNR
		public readonly Variable Variable;

		public VariableExpression(int index, TypeId type, Variable variable)
		{
			Variable = variable;
		}
#endif
		public VariableExpression(int index)
		{
			Index = index;
		}

#if CORE
		public override LsnValue Eval(IInterpreter i)
			=> i.GetVariable(Index);
#endif

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.Variable);
			writer.Write((ushort)Index);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}
	}
}
