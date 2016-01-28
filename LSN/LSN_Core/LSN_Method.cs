using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSN_Core.Expressions;

namespace LSN_Core
{
	/// <summary>
	/// A method written in LSN
	/// </summary>
	public class LSN_Method : Method
	{
		private List<Component> Components;

		private LSN_ResourceThing Resource;

		public override bool HandlesScope { get { return false; } }

		public LSN_Method(LSN_Type type, LSN_Type returnType, List<Component> components, LSN_ResourceThing res)
			:base(type,returnType)
		{
			Components = components;
			Resource = res;
		}

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
