using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// A function written in LSN.
	/// </summary>
	public class LSN_Function : Function
	{
		private List<Component> Components;

		private LSN_ResourceThing Resource;

		public override bool HandlesScope { get { return true; } }

		public override ILSN_Value Eval(Dictionary<string, ILSN_Value> args, IInterpreter i)
		{
			i.EnterFunctionScope(Resource?.GetEnvironment() ?? LSN_Environment.Default);
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
