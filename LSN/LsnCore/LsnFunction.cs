using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// A function written in LSN.
	/// </summary>
	[Serializable]
	public class LsnFunction : Function
	{
		/// <summary>
		/// This should only be set from within LSNr, where function bodies are parsed.
		/// </summary>
		public Statement[] Code;
		

		public override bool HandlesScope { get { return true; } }


		public LsnFunction(List<Parameter> parameters, LsnType returnType, string name,LsnEnvironment environment
			)
			:base(new FunctionSignature(parameters, name, returnType?.Id))
		{
			Environment = environment;
		}

		public LsnFunction(List<Parameter> parameters, TypeId returnType, string name,LsnEnvironment environment
			)
			: base(new FunctionSignature(parameters, name, returnType))
		{
			Environment = environment;
		}


		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
		{
			i.Run(Code, Environment, StackSize, args);
			i.ExitFunctionScope();
			return i.ReturnValue;
		}
    }
}
