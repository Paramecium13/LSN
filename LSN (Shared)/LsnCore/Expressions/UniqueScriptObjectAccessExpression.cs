﻿using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class UniqueScriptObjectAccessExpression : Expression
	{
		public override bool IsPure => false;

		public UniqueScriptObjectAccessExpression(TypeId type)
		{
			Type = type;
		}

#if CORE
		public override LsnValue Eval(IInterpreter i)
			=> new LsnValue(i.GetUniqueScriptObject(Type));
#endif

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.UniqueScriptObjectAccess);
			resourceSerializer.WriteTypeId(Type, writer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}
	}
}
