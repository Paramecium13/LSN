using System;
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
	public sealed class PropertyAccessExpression : IExpression
	{
		private IExpression ScriptObject;
		internal readonly int Index;

		public bool IsPure => false;

		private readonly TypeId _Type;
		public TypeId Type => _Type;

		public PropertyAccessExpression(IExpression scrObj, int index, TypeId type)
		{
			ScriptObject = scrObj; Index = index; _Type = type;
		}

		internal PropertyAccessExpression(IExpression scrObj, int index)
		{
			ScriptObject = scrObj; Index = index;
		}

		public bool Equals(IExpression other)
		{
			var o = other as PropertyAccessExpression;
			if (o == null) return false;
			return Index == o.Index && ScriptObject.Equals(o.ScriptObject);
		}

		public LsnValue Eval(IInterpreter i)
			=> (ScriptObject.Eval(i).Value as ScriptObject).GetPropertyValue(Index);

		public IExpression Fold() => this;

		public bool IsReifyTimeConst() => false;

		public void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (ScriptObject == oldExpr)
				ScriptObject = newExpr;
			else
				ScriptObject.Replace(oldExpr, newExpr);
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.PropertyAccess);
			writer.Write((ushort)Index);
			ScriptObject.Serialize(writer, resourceSerializer);
		}

		public IEnumerator<IExpression> GetEnumerator()
		{
			yield return ScriptObject;
			foreach (var expr in ScriptObject.SelectMany(e => e))
				yield return expr;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
