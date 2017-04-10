using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;

namespace LsnCore.Expressions
{
	[Serializable]
	public sealed class HostInterfaceAccessExpression : IExpression
	{
		public bool IsPure => false;

		private readonly TypeId _Type;
		public TypeId Type => _Type;

		private readonly IExpression _ScriptObject;


		public HostInterfaceAccessExpression(IExpression scrObj, TypeId type)
		{
			_ScriptObject = scrObj; _Type = type;
		}


		public bool Equals(IExpression other)
		{
			var o = other as HostInterfaceAccessExpression;
			if (o == null) return false;
			return _ScriptObject.Equals(o._ScriptObject);
		}

		public LsnValue Eval(IInterpreter i)
		{
			return (_ScriptObject.Eval(i).Value as ScriptObject).GetHost();
		}

		public IExpression Fold() => this;

		public bool IsReifyTimeConst() => false;

		public void Replace(IExpression oldExpr, IExpression newExpr) {}// _ScriptObject should be a variable access expression, accessing the 'self' parameter of
																		// a script object method.
	}
}
