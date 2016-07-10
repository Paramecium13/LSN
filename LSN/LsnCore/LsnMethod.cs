﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LsnCore
{
	/// <summary>
	/// A method written in LSN
	/// </summary>
	[Serializable]
	public class LsnMethod : Method
	{
		private List<Component> Components;

		private LsnResourceThing Resource;

		public override bool HandlesScope { get { return false; } }

		public LsnMethod(LsnType type, LsnType returnType, List<Component> components, LsnResourceThing res, 
			List<Parameter> paramaters = null)
			:base(type,returnType, paramaters ?? new List<Parameter>() { new Parameter("self", type, null, 0) })
		{
			Components = components;
			Resource = res;
		}

		public override ILsnValue Eval(Dictionary<string, ILsnValue> args, IInterpreter i)
		{
			i.EnterFunctionScope(Resource?.GetEnvironment() ?? LsnEnvironment.Default);
			foreach (var pair in args) i.AddVariable(pair.Key, pair.Value);
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