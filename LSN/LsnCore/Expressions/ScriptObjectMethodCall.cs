using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
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
			var parameters = Parameters.Select(p => p.Eval(i)).ToArray();
			return (parameters[0].Value as ScriptObject).GetMethod(Name).Eval(parameters, i);
		}

		public override IExpression Fold()
			=> new ScriptObjectMethodCall(Parameters.Select(p => p.Fold()).ToArray(), Name);

		public override bool IsReifyTimeConst() => false;
	}
}
