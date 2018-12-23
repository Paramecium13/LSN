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
	/// <summary>
	/// Access a value in a collection.
	/// </summary>
	public sealed class CollectionValueAccessExpression : Expression
	{
		public IExpression Collection;
		public IExpression Index;

		public override bool IsPure => true;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collection"> The collection expression.</param>
		/// <param name="index"> The index expression.</param>
		/// <param name="type"> The type of the value contained in the collection.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CollectionValueAccessExpression(IExpression collection, IExpression index, TypeId type)
		{
			Collection = collection; Index = index; Type = type;
		}

		public CollectionValueAccessExpression(IExpression collection, IExpression index)
		{
			Collection = collection; Index = index;
		}

		public override bool Equals(IExpression other)
		{
			var e = other as CollectionValueAccessExpression;
			if (e == null) return false;
			return e.Collection == Collection && e.Index == Index;
		}

#if CORE
		public override LsnValue Eval(IInterpreter i)
		{
			return (Collection.Eval(i).Value as ICollectionValue).GetValue(Index.Eval(i).IntValue);
		}
#endif

		public override IExpression Fold()
		{
			var c = Collection.Fold();
			var i = Index.Fold();
			if(i != Index || c != Collection)
			{
				IExpression expr;       // typeof(ICollectionValue).IsAssignableFrom(c.GetType())
				var cl = c as ICollectionValue;
				if (i.IsReifyTimeConst() && cl != null)
				{
					try
					{
						expr = cl.GetValue(((LsnValue)i).IntValue);
					}
					catch (Exception)
					{
						expr = new CollectionValueAccessExpression(c, i, Type);
					}
				}
				else expr = new CollectionValueAccessExpression(c, i, Type);
				return expr;
			}
			return this;
		}

		public override bool IsReifyTimeConst()
			=> Collection.IsReifyTimeConst() && Index.IsReifyTimeConst();

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Collection.Equals(oldExpr)) Collection = newExpr;
			if (Index.Equals( oldExpr)) Index = newExpr;
		}

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.CollectionValueAccess);
			Collection.Serialize(writer, resourceSerializer);
			Index.Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Collection;
			foreach (var expr in Collection.SelectMany(e => e))
				yield return expr;
			yield return Index;
			foreach (var expr in Index.SelectMany(e => e))
				yield return expr;
		}
	}
}
