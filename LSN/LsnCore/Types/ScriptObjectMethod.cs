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


		public readonly bool IsAbstract;


		internal IReadOnlyList<Component> Components; // Assigned in LSNr.
		


		public ScriptObjectMethod(TypeId type, TypeId returnType, IList<Parameter> parameters, LsnEnvironment environment,
			bool isVirtual, bool isAbstract, string name)
			:base(type,returnType,name,parameters)
		{
			if(Parameters[0].Name != "self")
				throw new ApplicationException("");
			Environment = environment;
			IsVirtual = isVirtual;
			IsAbstract = isAbstract;
			if (IsAbstract && !IsVirtual) throw new ArgumentException();
		}


		public Expression CreateScriptObjectMethodCall(IExpression[] parameters)
		{
			if (parameters.Length != Parameters.Count)
				throw new ApplicationException();
			if (parameters[0].Type != TypeId)
				throw new ApplicationException();

			//if (IsVirtual)
				return new ScriptObjectVirtualMethodCall(parameters, Name);
			//else
				//return new MethodCall(this, parameters); Can't do this, would result in this method being serialized along with it's call.
		}

		public override LsnValue Eval(LsnValue[] args, IInterpreter i)
		{
			i.EnterFunctionScope(Environment, StackSize);
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
