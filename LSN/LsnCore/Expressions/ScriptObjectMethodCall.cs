using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	public class ScriptObjectMethodCall : Expression
	{
		private readonly IExpression[] Parameters;

		//private readonly bool IsVirtual;

		private readonly string Name;
		

		public override bool IsPure => false;


		public ScriptObjectMethodCall(IExpression[] parameters, string name)
		{
			Parameters = parameters; Name = name;
		}


		public override LsnValue Eval(IInterpreter i)
		{
			throw new NotImplementedException();
		}


		public override IExpression Fold()
		{
			throw new NotImplementedException();
		}


		public override bool IsReifyTimeConst() => false;
	}
}
