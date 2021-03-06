﻿using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Collections;

namespace LsnCore.Expressions
{
	public sealed class ListConstructor : IExpression
	{
		public bool IsPure => false;

		public TypeId Type { get; }

		private readonly TypeId GenericTypeId;

		public ListConstructor(TypeId genericTypeId)
		{
			GenericTypeId = genericTypeId;
			Type = LsnListGeneric.Instance.GetType(new TypeId[] { genericTypeId }).Id;
		}

		public ListConstructor(LsnListType listType)
		{
			GenericTypeId = listType.GenericId;
			Type = listType.Id;
		}

		public bool Equals(IExpression other)
		{
			return this == other;
		}

#if CORE
		public LsnValue Eval(IInterpreter i)
		{
			return new LsnValue(new LsnList(Type));
		}
#endif

		public IExpression Fold() => this;


		public bool IsReifyTimeConst() => false;


		public void Replace(IExpression oldExpr, IExpression newExpr)
		{}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.ListConstructor);
			resourceSerializer.WriteTypeId(GenericTypeId, writer);
		}

		public IEnumerator<IExpression> GetEnumerator()
		{
			yield return null;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
