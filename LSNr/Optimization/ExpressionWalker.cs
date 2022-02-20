using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Optimization
{
	public abstract class ExpressionWalker
	{

		public IExpression Walk(IExpression e)
		{
			/*var bin = e as BinaryExpression;
			if(bin != null)
				return WalkBinExp(bin);*/

			switch (e)
			{
				case CollectionValueAccessExpression cva:
					WalkCvaExp(cva);
					return e;
				case FieldAccessExpression fa:
					WalkFieldAccess(fa);
					return e;
				case FunctionCall fc:
					WalkFuncCall(fc);
					return e;
				case GetExpression gt:
					WalkGet(gt);
					return e;
				case MethodCall mc:
					WalkMethodCall(mc);
					return e;
				case StructConstructor rc:
					return e;
				case RecordConstructor sc:
					return e;
				case VariableExpression vb:
					return e;
				default:
					return e;
			}
		}

		/*protected virtual IExpression WalkBinExp(BinaryExpression e)
		{
			var v = View(e);
			if(v != e)
			{
				Walk(v);
				return v;
			}
			Walk(e.Left); Walk(e.Right);
			return e;
		}*/


		//protected virtual IExpression View(BinaryExpression b) { return b; }


		protected virtual void WalkCvaExp(CollectionValueAccessExpression c)
		{
			c.Collection = Walk(c.Collection);
			c.Index = Walk(c.Index);
		}


		protected virtual IExpression WalkFieldAccess(FieldAccessExpression f)
		{
			f.Value = Walk(f.Value);
			if(f.Value is LsnValue value)
				return ((IHasFieldsValue)value.Value).GetFieldValue(f.Index);
			return f;
		}


		protected virtual void WalkFuncCall(FunctionCall f)
		{
			for (var i = 0; i < f.Args.Length; i++)
				f.Args[i] = Walk(f.Args[i]);
		}


		protected virtual void WalkGet(GetExpression g)
		{

		}


		protected virtual void WalkMethodCall(MethodCall m)
		{
			for (var i = 0; i < m.Args.Length; i++)
				m.Args[i] = Walk(m.Args[i]);
		}

		
		protected virtual IExpression WalkRecordConstructor(StructConstructor rc)
		{
			foreach (var exp in rc.Args)
				Walk(exp);
			return rc;
		}


		protected virtual IExpression WalkStructConstuctor(RecordConstructor sc)
		{
			foreach (var exp in sc.Args)
				Walk(exp);

			return sc;
		}


		protected virtual void View(VariableExpression v){ }
	}
}
