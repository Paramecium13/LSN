using LsnCore.Expressions;
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
			var bin = e as BinaryExpression;
			if(bin != null)
				return WalkBinExp(bin);
			var cva = e as CollectionValueAccessExpression;
			if(cva != null)
			{

				return e;
			}
			var fa = e as FieldAccessExpression;
			if(fa != null)
			{

				return e;
			}
			var fc = e as FunctionCall;
			if(fc != null)
			{

				return e;
			}
			var gt = e as GetExpression;
			if(gt != null)
			{

				return e;
			}
			var mc = e as MethodCall;
			if(mc != null)
			{

				return e;
			}
			var or = e as OrExpression;
			if(or != null)
			{

				return e;
			}
			var rc = e as RecordConstructor;
			if(rc != null)
			{

				return e;
			}
			var sc = e as StructConstructor;
			if(sc != null)
			{

				return e;
			}
			var vb = e as VariableExpression;
			if(vb != null)
			{

				return e;
			}
			return e;
		}

		protected virtual IExpression WalkBinExp(BinaryExpression e)
		{
			var v = View(e);
			if(v != e)
			{
				Walk(v);
				return v;
			}
			Walk(e.Left); Walk(e.Right);
			return e;
		}


		protected virtual IExpression View(BinaryExpression b) { return b; }


		protected virtual void WalkCvaExp(CollectionValueAccessExpression c)
		{
			Walk(c.Collection);Walk(c.Index);
		}


		protected virtual void WalkFieldAccess(FieldAccessExpression f)
		{
			Walk(f.Value);
		}


		protected virtual void WalkFuncCall(FunctionCall f)
		{
			foreach (var exp in f.Args.Values)
				Walk(exp);
		}


		protected virtual void WalkGet(GetExpression g)
		{

		}


		protected virtual void WalkMethodCall(MethodCall m)
		{
			foreach (var exp in m.Args.Values)
				Walk(exp);
			Walk(m.Value);
		}


		protected virtual void WalkOrExp(OrExpression or)
		{
			Walk(or.Left);Walk(or.Right);
		}


		protected virtual void WalkRecordConstructor(RecordConstructor rc)
		{
			foreach (var exp in rc.Args.Values)
				Walk(exp);
		}


		protected virtual void WalkStructConstuctor(StructConstructor sc)
		{
			foreach (var exp in sc.Args.Values)
				Walk(exp);
			foreach (var exp in sc.ArgsB)
				Walk(exp);
		}


		protected virtual void View(VariableExpression v){ }
	}
}
