using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public class ScriptObjectVirtualMethodCall : Expression
	{
		private readonly IExpression[] Parameters;

		//private readonly bool IsVirtual;

		private readonly string Name;
		

		public override bool IsPure => false;


		public ScriptObjectVirtualMethodCall(IExpression[] parameters, string name)
		{
			Parameters = parameters; Name = name;
		}


		public override LsnValue Eval(IInterpreter i)
		{
			var parameters = Parameters.Select(p => p.Eval(i)).ToArray();
			return (parameters[0].Value as ScriptObject).GetMethod(Name).Eval(parameters, i);
		}

		public override IExpression Fold()
			=> new ScriptObjectVirtualMethodCall(Parameters.Select(p => p.Fold()).ToArray(), Name);

		public override bool IsReifyTimeConst() => false;
	}
}
