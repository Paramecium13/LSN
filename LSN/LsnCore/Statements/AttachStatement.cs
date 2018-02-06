using LsnCore.Expressions;
using LsnCore.Types;
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
		private readonly ScriptClass ScriptClass;
		public IExpression[] PropertyExpressions;
		public IExpression[] ConsructorArguments;
		public IExpression HostExpression;

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			throw new NotImplementedException();
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}
	}
}
