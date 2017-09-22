using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	[Serializable]
	public sealed class ListConstructor : IExpression
	{
		public bool IsPure => false;

		private readonly TypeId _Type;
		public TypeId Type => _Type;

		private readonly TypeId GenericTypeId;

		public ListConstructor(TypeId genericTypeId)
		{
			GenericTypeId = genericTypeId;
			_Type = LsnListGeneric.Instance.GetType(new List<TypeId> { genericTypeId }).Id;
		}

		public ListConstructor(LsnListType listType)
		{
			GenericTypeId = listType.GenericId;
			_Type = listType.Id;
		}

		public bool Equals(IExpression other)
		{
			return this == other;
		}

		public LsnValue Eval(IInterpreter i)
		{
			return new LsnValue(new LsnList(Type));
		}

		public IExpression Fold() => this;


		public bool IsReifyTimeConst() => false;


		public void Replace(IExpression oldExpr, IExpression newExpr)
		{}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.ListConstructor);
			writer.Write(GenericTypeId.Name);
		}
	}
}
