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

		public override ILSN_Value Eval(Dictionary<string, IExpression> args, IInterpreter i)
		{
			i.EnterFunctionScope(Resource.GetEnvironment());
			foreach (var pair in args) i.AddVariable(pair.Key, pair.Value.Eval(i));
			int index = 0;
			while (Components[index++].Interpret(i)) ;
			//for (int x = 0; x < Components.Count; x++) Components[x].Interpret(i); 
			i.ExitFunctionScope();
			return i.ReturnValue;
		}
    }
}
