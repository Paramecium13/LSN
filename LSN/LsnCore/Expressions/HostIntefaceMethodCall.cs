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
	public sealed class HostIntefaceMethodCall : IExpression
	{
		public bool IsPure => false;

		private readonly TypeId _Type;
		public TypeId Type => _Type;

		private readonly string Name;

		private IExpression ScriptObject;

		private IExpression[] Arguments;


		public HostIntefaceMethodCall(MethodDefinition def, IExpression scriptObject, IExpression[] args)
		{
			_Type = def.ReturnType; Name = def.Name; ScriptObject = scriptObject; Arguments = args;
		}


		public bool Equals(IExpression other)
		{
			return this == other; // Replace w/ more in depth comparison? 
		}

		public LsnValue Eval(IInterpreter i)
		{
			return (ScriptObject.Eval(i).Value as ScriptObject).ExecuteHostInterfaceMethod(Name, Arguments.Select(a => a.Eval(i)).ToArray());
		}

		public IExpression Fold()
		{
			ScriptObject = ScriptObject.Fold();
			for (int i = 0; i < Arguments.Length; i++)
				Arguments[i] = Arguments[i].Fold();

			return this;
		}

		public bool IsReifyTimeConst() => false;

		public void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (ScriptObject == oldExpr)
				ScriptObject = newExpr;
			for (int i = 0; i < Arguments.Length; i++)
				if (Arguments[i] == oldExpr) Arguments[i] = newExpr; 
		}
	}
}
