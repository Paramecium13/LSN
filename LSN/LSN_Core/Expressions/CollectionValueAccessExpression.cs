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
				IExpression expr;
				if (i.IsReifyTimeConst() && typeof(ICollectionValue).IsAssignableFrom(c.GetType()))
				{
					try
					{
						expr = ((ICollectionValue)c).GetValue((ILSN_Value)i);
					}
					catch (Exception e)
					{
						expr = new CollectionValueAccessExpression(c, i, Type);
						throw;
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
