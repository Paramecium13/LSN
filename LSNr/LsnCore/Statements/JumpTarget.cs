using System;
using System.Collections.Generic;
using System.Linq;
using LsnCore.Expressions;
#if LSNR
using LSNr;
#endif
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public class JumpToTargetStatement : Statement
	{
		private int Index;

		public JumpToTargetStatement(int index)
		{
			Index = index;
		}
#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			i.NextStatement = i.GetVariable(Index).IntValue;
			return InterpretValue.Base;
		}
#endif
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
#if LSNR
			if (!(oldExpr is VariableExpression vOld) || vOld.Index != Index) return;
			switch (newExpr)
			{
				case VariableExpression vNew:
					Index = vNew.Index;
					//vNew.Variable.AddUser(this);
					break;
				case LsnValue val:
					Index = val.IntValue;
					break;
				default:
					throw new InvalidOperationException();
			}
#endif
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((ushort)StatementCode.JumpToTarget);
			writer.Write((ushort)Index);
		}

		public override IEnumerator<IExpression> GetEnumerator()// => Enumerable.Empty<IExpression>().GetEnumerator();
		{
			yield break;
		}

		protected override IEnumerable<PreInstruction> GetInstructions(string target)
		{
			yield return new SimplePreInstruction(OpCode.JumpToTarget, 0);
		}
	}

	public sealed class SetTargetStatement : Statement, IHasTargetStatement
	{
		public int Target { get; set; }
#if LSNR
		public readonly Variable Variable;

		private int Index => Variable.Index;

		public SetTargetStatement(Variable variable)
		{
			Variable = variable ?? throw new ArgumentNullException(nameof(variable));
		}

		public SetTargetStatement(ushort index)
		{
			Variable = new Variable("Jump Target", LsnType.int_, index, true);
		}
#else
		private int Index;

		public SetTargetStatement(int index)
		{
			Index = index;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.SetVariable(Index, new LsnValue(Target));
			return InterpretValue.Base;
		}
#endif
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
#if LSNR
			if (Variable.Name != "Jump Target" || !(oldExpr is VariableExpression vOld) || vOld.Index != Index) return;
			switch (newExpr)
			{
				case VariableExpression vNew:
					Variable.Index = vNew.Index;
					//vNew.Variable.AddUser(this);
					break;
				case LsnValue val:
					Variable.Index = val.IntValue;
					break;
				default:
					throw new InvalidOperationException();
			}
#endif
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
#if LSNR
			yield return Variable.AccessExpression; // ????
#else
			yield break;
#endif
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((ushort)StatementCode.SetTarget);
			writer.Write((ushort)Index);
			writer.Write(Target);
		}

	}
}