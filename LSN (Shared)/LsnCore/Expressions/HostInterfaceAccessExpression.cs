﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class HostInterfaceAccessExpression : IExpression
	{
		public bool IsPure => false;

		private readonly TypeId _Type;
		public TypeId Type => _Type;

		public HostInterfaceAccessExpression(TypeId type)
		{
			_Type = type;
		}

		public HostInterfaceAccessExpression(){}

		public bool Equals(IExpression other)
		{
			return other is HostInterfaceAccessExpression;
		}

#if CORE
		public LsnValue Eval(IInterpreter i)
		{
			return (i.GetVariable(0).Value as ScriptObject).GetHost();
		}
#endif

		public IExpression Fold() => this;

		public bool IsReifyTimeConst() => false;

		public void Replace(IExpression oldExpr, IExpression newExpr) {}// _ScriptObject should be a variable access expression, accessing the 'self' parameter of
																		// a script object method.

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.HostInterfaceAccess);
		}

		public IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
