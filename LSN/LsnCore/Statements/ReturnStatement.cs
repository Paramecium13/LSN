﻿using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	/// <summary>
	/// Returns from a function, possibly with a value
	/// </summary>
	public class ReturnStatement : Statement
	{
		private IExpression _Value;
		public IExpression Value { get { return _Value; } set { _Value = value; } }

		public ReturnStatement(IExpression e)
		{
			Value = e;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			if(Value != null) i.ReturnValue = Value.Eval(i);
			return InterpretValue.Return;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if(Value != null)
			{
				if (oldExpr.Equals(Value))
					Value = newExpr;
				else
					Value.Replace(oldExpr, newExpr);
			}
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			if(Value == null)
				writer.Write((byte)StatementCode.Return);
			else
			{
				writer.Write((byte)StatementCode.ReturnValue);
				Value.Serialize(writer, resourceSerializer);
			}
		}
	}
}
