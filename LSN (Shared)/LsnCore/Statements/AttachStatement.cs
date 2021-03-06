﻿using LsnCore.Expressions;
using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Statements
{
	public class AttachStatement : Statement
	{
		private readonly TypeId ScriptClass;
		private readonly IExpression[] ConstructorArguments;
		private IExpression HostExpression;

		public AttachStatement(TypeId scriptClass, IExpression[] args, IExpression host)
		{
			ScriptClass = scriptClass; ConstructorArguments = args; HostExpression = host;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			var obj = (ScriptClass.Type as ScriptClass)
				.Construct(ConstructorArguments.Select(e => e.Eval(i)).ToArray(),
				i, HostExpression.Eval(i).Value as IHostInterface);
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			for (int i = 0; i < ConstructorArguments.Length; i++)
			{
				if (ConstructorArguments[i].Equals(oldExpr))
					ConstructorArguments[i] = newExpr;
				else ConstructorArguments[i].Replace(oldExpr, newExpr);
			}
			if (HostExpression.Equals(oldExpr))
				HostExpression = newExpr;
			else HostExpression.Replace(oldExpr, newExpr);
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.AttachNewScriptObject);

			resourceSerializer.WriteTypeId(ScriptClass, writer);

			writer.Write((byte)ConstructorArguments.Length);
			for (int i = 0; i < ConstructorArguments.Length; i++)
				ConstructorArguments[i].Serialize(writer, resourceSerializer);

			HostExpression.Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			for (int i = 0; i < ConstructorArguments.Length; i++)
			{
				yield return ConstructorArguments[i];
				foreach (var expr in ConstructorArguments[i].SelectMany(e => e))
					yield return expr;
			}
			yield return HostExpression;
			foreach (var expr in HostExpression.SelectMany(e => e))
				yield return expr;
		}
	}
}
