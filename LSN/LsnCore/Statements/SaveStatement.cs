using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public class SaveStatement : Statement
	{
		private readonly ushort[] VariableIndexes;
		private readonly string SaveId;

		public SaveStatement(ushort[] variableIndexes, string saveId)
		{
			VariableIndexes = variableIndexes; SaveId = saveId;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.SaveVariables(VariableIndexes, SaveId);
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.Save);
			writer.Write((ushort)VariableIndexes.Length);
			writer.Write(VariableIndexes);
			writer.Write(SaveId);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}
	}

	public class LoadStatement : Statement
	{
		private readonly ushort[] VariableIndexes;
		private readonly string SaveId;

		public LoadStatement(ushort[] variableIndexes, string saveId)
		{
			VariableIndexes = variableIndexes; SaveId = saveId;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.LoadVariables(VariableIndexes, SaveId);
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{ }

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.Load);
			writer.Write((ushort)VariableIndexes.Length);
			writer.Write(VariableIndexes);
			writer.Write(SaveId);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}
	}
}
