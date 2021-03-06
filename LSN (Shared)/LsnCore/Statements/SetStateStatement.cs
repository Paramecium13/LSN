﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Values;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	public sealed class SetStateStatement : Statement
	{
		private readonly int State;

		public SetStateStatement(int state)
		{
			State = state;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			(i.GetVariable(0).Value as ScriptObject).SetState(State);
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.SetState);
			writer.Write(State);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}
	}
}
