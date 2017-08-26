using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Statements;

namespace LsnCore
{
	/// <summary>
	/// A method written in LSN
	/// </summary>
	[Serializable]
	public class LsnMethod : Method
	{
		public Statement[] Code;

		private readonly LsnResourceThing Resource;

		public override bool HandlesScope { get { return false; } }

		public LsnMethod(LsnType type, LsnType returnType, Statement[] code, string name, LsnResourceThing res, 
			List<Parameter> paramaters = null)
			:base(type,returnType,name, paramaters ?? new List<Parameter>() { new Parameter("self", type.Id, LsnValue.Nil, 0) })
		{
			Code = code;
			Resource = res;
		}

		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
		{
			i.Run(Code, ResourceFilePath, StackSize, args);
			i.ExitFunctionScope();
			return i.ReturnValue;
		}
	}
}
