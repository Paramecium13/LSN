﻿using LsnCore.Expressions;
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
		public List<Component> Components;

		public LsnResourceThing Resource;

		public override bool HandlesScope { get { return true; } }


		public LsnFunction(List<Parameter> parameters, LsnType returnType, string name)
			:base(parameters)
		{
			ReturnType = returnType?.Id;
			Name = name;
		}

		public LsnFunction(List<Parameter> parameters, TypeId returnType, string name)
			: base(parameters)
		{
			ReturnType = returnType;
			Name = name;
		}

		public override LsnValue Eval(Dictionary<string, LsnValue> args, IInterpreter i)
		{
			i.EnterFunctionScope(Resource?.GetEnvironment() ?? LsnEnvironment.Default, StackSize);
			//ToDo: assign variables to stack;
			//foreach (var pair in args) i.AddVariable(pair.Key, pair.Value); // ToDo: remove AddVariable(...)
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
						throw new ApplicationException("This should not happen.");
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
