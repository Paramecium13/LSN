﻿using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	/// <summary>
	/// Access a value in a collection.
	/// </summary>
	[Serializable]
	public class CollectionValueAccessExpression : Expression
	{
		public IExpression Collection;
		public IExpression Index;

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

		public override bool Equals(IExpression other)
		{
			var e = other as CollectionValueAccessExpression;
			if (e == null) return false;
			return e.Collection == Collection && e.Index == Index;
		}

		public override ILsnValue Eval(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override IExpression Fold()
		{
			var c = Collection.Fold();
			var i = Index.Fold();
			if(i != Index || c != Collection)
			{
				IExpression expr;       // typeof(ICollectionValue).IsAssignableFrom(c.GetType())
				if (i.IsReifyTimeConst() && c is ICollectionValue)
				{
					try
					{
						expr = (c as ICollectionValue).GetValue((ILsnValue)i);
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
	}
}
