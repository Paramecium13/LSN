using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Statements
{
	/// <summary>
	/// Returns from a function, possibly with a value
	/// </summary>
	[Serializable]
	public class ReturnStatement : Statement
	{
		private IExpression _Value;
		public IExpression Value { get { return _Value; } set { _Value = value; } }

		public ReturnStatement(IExpression e)
		{
			Value = e;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			if(Value != null) i.ReturnValue = Value.Eval(i);
			return InterpretValue.Return;
		}
	}
}
