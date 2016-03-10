using LSN_Core.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	/// <summary>
	/// Access a value in a collection.
	/// </summary>
	public class CollectionValueAccessExpression : Expression
	{
		private IExpression Collection;
		private IExpression Index;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collection"> The collection expression.</param>
		/// <param name="index"> The index expression.</param>
		/// <param name="type"> The type of the value contained in the collection.</param>
		public CollectionValueAccessExpression(IExpression collection, IExpression index, LSN_Type type)
		{
			Collection = collection; Index = index; Type = type;
		}

		public override ILSN_Value Eval(IInterpreter i)
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
						expr = (c as ICollectionValue).GetValue((ILSN_Value)i);
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
	}
}
