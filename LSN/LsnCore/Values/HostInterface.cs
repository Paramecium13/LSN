using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LsnCore.Values
{
	// An implementation of IHostInterface.
	public class HostInterface : IHostInterface
	{
		public virtual bool BoolValue => true;

		public bool IsPure => false;

		// The actual type...

		private readonly TypeId _Type;

		public TypeId Type => _Type;


		protected readonly Dictionary<string, Func<LsnValue[], LsnValue>> Methods = new Dictionary<string, Func<LsnValue[], LsnValue>>();

		// Events


		public bool Equals(IExpression other) => other == this;
		public ILsnValue Clone() => this;
		public LsnValue Eval(IInterpreter i) => new LsnValue(this);
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => false;
		public void Replace(IExpression oldExpr, IExpression newExpr){}

		public void SubscribeToEvent(string eventName, object eventListener)
		{
			throw new NotImplementedException();
		}

		public void UnsubscribeToEvent(string eventName, object eventListener)
		{
			throw new NotImplementedException();
		}

		public LsnValue CallMethod(string name, LsnValue[] arguments)
		{
			// Check if the method is defined by the type.
			if (Methods.ContainsKey(name))
				return Methods[name](arguments);
			else throw new InvalidOperationException($"The method {name} has not been registered.");
		}


		// The following methods are meant to be called by the game engine.

		public virtual void RegisterMethod(string methodName, Func<LsnValue[],LsnValue> fn)
		{
			// Check if it is defined by the type.
			if (!Methods.ContainsKey(methodName))
				Methods.Add(methodName, fn);
			else
				throw new InvalidOperationException($"The method {methodName} has already been registered.");
		}


		public virtual void FireEvent(string eventName)
		{
			// Check if the event is defined by the type (?)
			throw new NotImplementedException();
		}

	}
}
