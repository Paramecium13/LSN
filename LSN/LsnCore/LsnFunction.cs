using LsnCore.Expressions;
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
		public IReadOnlyList<Component> Components;
		

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
			i.EnterFunctionScope(Environment ?? LsnEnvironment.Default, StackSize);
			//ToDo: assign variables to stack;
			//foreach (var pair in args) i.AddVariable(pair.Key, pair.Value); // ToDo: remove AddVariable(...)
			for (int argI = 0; argI < args.Length; argI++)
				i.SetValue(argI, args[argI]);

			for (int x = 0; x < Components.Count; x++)
			{
				var val = Components[x].Interpret(i);
				switch (val)
				{
					case InterpretValue.Base:
						break;
					case InterpretValue.Next:
						throw new ApplicationException("This should not happen.");
					case InterpretValue.Break:
						throw new ApplicationException("I SHALL NOT BE BROKEN!!!\nYou cannot have a break statement directly in a function.");
					case InterpretValue.Return: //Exit the function.
						x = Components.Count; // This breaks the for loop.
						break;
					default:
						throw new ApplicationException("This should not happen.");
				}
			}
			i.ExitFunctionScope();
			return i.ReturnValue;
		}
    }
}
