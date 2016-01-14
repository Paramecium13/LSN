using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace LSN_Core.Expressions
{

	public abstract class LSN_Expression
	{
		public virtual LSN_Type Type { get; set; }

		public abstract bool IsCompileConst();

		public abstract LSN_BoundedInstance Eval();

		public abstract LSN_Expression Fold();

		public abstract string Translate(int n=0);
	}
	
	public abstract class LSN_BinaryExpression : LSN_Expression
	{
		public LSN_Expression LeftSide { get; set; }
		public LSN_Expression RightSide { get; set; }
		protected abstract string Operator { get; }

		public override bool IsCompileConst()
		{
			return LeftSide.IsCompileConst() && RightSide.IsCompileConst();
		}

		public override string Translate(int n = 0)
		{
			return new string('\t', n) + LeftSide.Translate() + Operator + RightSide.Translate();
		}

	}

	public class LSN_Addition : LSN_BinaryExpression
	{
		protected override string Operator { get { return " + "; } }

		public LSN_Addition(LSN_Expression l, LSN_Expression r)
		{
			LeftSide = l;
			RightSide = r;
		}

		public override LSN_BoundedInstance Eval()
		{
			if (LeftSide.IsCompileConst() && RightSide.IsCompileConst())
			{
				if (LSN_Type.Adders.ContainsKey(LeftSide.Type))
				{
					return LSN_Type.Adders[LeftSide.Type](LeftSide.Eval(), RightSide.Eval());
				}
			}
			return null;
		}

		public override LSN_Expression Fold()
		{
			var l = LeftSide.Fold();
			var r = RightSide.Fold();
			if(l.IsCompileConst() && r.IsCompileConst())
			{
				if (LSN_Type.Adders.ContainsKey(l.Type))
				{
					return LSN_Type.Adders[l.Type](l.Eval(), r.Eval());
				}
			}
			return new LSN_Addition(l, r);
		}

		
	}

	public class LSN_BoundedInstance : LSN_Expression
	{
		public Type MyBoundType { get; protected set; }
		public object Value { get; protected set; }

		public LSN_BoundedInstance(LSN_Type ty,object value)
		{
			if (!Type.IsBounded)
			{
				throw new Exception();
			}
			MyBoundType = Type.GetType().GetGenericArguments()[0];
			if(value.GetType() != MyBoundType)
			{
				throw new Exception();
			}
			Value = value;
		}

		public T GetValue<T>() => (T)Value;

		public override bool IsCompileConst() => true;

		public override LSN_BoundedInstance Eval() => this;

		public override LSN_Expression Fold() => this;

		public override string Translate(int n = 0)
		{
			return new string('\t', n) + LSN_Type.Translators[Type](this); 
        }
	}

}
