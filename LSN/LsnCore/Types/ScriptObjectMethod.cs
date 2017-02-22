using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public class ScriptObjectMethod : Method
	{
		public override bool HandlesScope => false;

		public readonly bool IsVirtual;

		private readonly IReadOnlyList<Component> Components;

		private readonly LsnResourceThing Resource;

		public ScriptObjectMethod(TypeId type, TypeId returnType, IList<Parameter> parameters, IReadOnlyList<Component> components,
			LsnResourceThing res, bool isVirtual, string name)
			:base(type,returnType,name,parameters)
		{
			if(Parameters[0].Name != "self")
				throw new ApplicationException("");
			if(Parameters[1].Name != "host")
				throw new ApplicationException("");
			Resource = res;
			Components = components;
			IsVirtual = isVirtual;
		}


		/*public ScriptObjectMethodCall CreateScriptObjectMethodCall(IExpression[] parameters)
		{
			if (parameters.Length != Parameters.Count)
				throw new ApplicationException();
			if (parameters[0].Type != TypeId)
				throw new ApplicationException();

			return new ScriptObjectMethodCall(parameters, Name);
		}*/

		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
		{
			i.EnterFunctionScope(Resource?.GetEnvironment() ?? LsnEnvironment.Default, StackSize);
			for (int argI = 0; argI < args.Length; argI++) // Assign variables to the stack.
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
