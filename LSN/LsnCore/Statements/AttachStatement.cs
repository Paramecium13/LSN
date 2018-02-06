using LsnCore.Expressions;
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
		private readonly IExpression[] PropertyExpressions;
		private readonly IExpression[] ConstructorArguments;
		private IExpression HostExpression;

		public AttachStatement(TypeId scriptClass, IExpression[] properties, IExpression[] args, IExpression host)
		{
			ScriptClass = scriptClass; PropertyExpressions = properties; ConstructorArguments = args; HostExpression = host;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			var obj = (ScriptClass.Type as ScriptClass).Construct(PropertyExpressions.Select(e => e.Eval(i)).ToArray(),
				ConstructorArguments.Select(e => e.Eval(i)).ToArray(), i, HostExpression.Eval(i).Value as IHostInterface);
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			for (int i = 0; i < PropertyExpressions.Length; i++)
			{
				if (PropertyExpressions[i].Equals(oldExpr))
					PropertyExpressions[i] = newExpr;
				else PropertyExpressions[i].Replace(oldExpr, newExpr);
			}
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

			writer.Write((byte)PropertyExpressions.Length);
			for (int i = 0; i < PropertyExpressions.Length; i++)
				PropertyExpressions[i].Serialize(writer, resourceSerializer);

			writer.Write((byte)ConstructorArguments.Length);
			for (int i = 0; i < ConstructorArguments.Length; i++)
				ConstructorArguments[i].Serialize(writer, resourceSerializer);

			HostExpression.Serialize(writer, resourceSerializer);
		}
	}
}
