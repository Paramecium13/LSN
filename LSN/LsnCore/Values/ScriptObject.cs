using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LsnCore.Values
{
	[Serializable]
	public sealed class ScriptObject : ILsnValue, IHasMutableFieldsValue
	{

		private readonly LsnValue[] Properties;

		private readonly LsnValue[] Fields;


		private readonly ScriptObjectType ScObjType;

		private readonly IHostInterface Host;


		private int CurrentStateIndex;
		private ScriptObjectState CurrentState;

		public ScriptObject(LsnValue[] properties, LsnValue[] fields, ScriptObjectType type, int currentState, IHostInterface host = null)
		{
			Properties = properties; Fields = fields; Type = type.Id; ScObjType = type; CurrentStateIndex = currentState;
			CurrentState = ScObjType.GetState(CurrentStateIndex);
			if (host != null)
			{
				// Check types
				if (ScObjType.HostInterface == null)
					throw new ArgumentException("This type of ScriptObject does not have a host.", "host");
				if (!ScObjType.HostInterface.Equals(host.Type))
					throw new ArgumentException($"Invalid HostInterface type. Expected {ScObjType.HostInterface.Name}. Recieved {host.Type.Name}.", "host");

				Host = host;

				//TODO: Subscribe to events.
			}
			else
			{
				if (ScObjType.HostInterface != null) throw new ArgumentException("This type of ScriptObject cannot survive without a host.", "host");
			}
		}


		public bool BoolValue => true;
		public bool IsPure => false;
		public TypeId Type { get; private set; }
		public ILsnValue Clone() => this;
		public bool Equals(IExpression other) => false;
		public LsnValue Eval(IInterpreter i) => new LsnValue(this);
		public IExpression Fold() => new LsnValue(this);
		public bool IsReifyTimeConst() => false;
		public void Replace(IExpression oldExpr, IExpression newExpr){}


		public LsnValue GetFieldValue(int index)
			=> Fields[index];


		public void SetFieldValue(int index, LsnValue value)
		{
			Fields[index] = value;
		}


		internal LsnValue GetPropertyValue(int index)
			=> Properties[index];
		

		internal ScriptObjectMethod GetMethod(string methodName)
		{
			//Is the method virtual?
			//	Does the current state override it?
			//		Return the current state's implementation
			//	Else Does the Type have an implementation?
			//		Return that
			//	Else
			//	Throw exception
			//Else
			//	Return the type's implementation.

			if (ScObjType.HasMethod(methodName))
			{
				var method = ScObjType.GetMethod(methodName);
				if (method.IsVirtual)
				{
					if (CurrentState.HasMethod(methodName))
						return CurrentState.GetMethod(methodName);
					if (!method.IsAbstract)
						return method;
					throw new ApplicationException("...");
				}
				return method;
			}
			throw new ArgumentException($"The ScriptObject type \"{ScObjType.Name}\" does not have a method named \"{methodName}\".",nameof(methodName));
		}


		public LsnValue ExecuteHostInterfaceMethod(string name, LsnValue[] values)
		{
			return Host.CallMethod(name, values);
		}


		internal void SetState(int index)
		{
			//TODO: Unsubscribe from old state's event subscriptions (if valid). Run old state exit method.
			CurrentStateIndex = index;
			CurrentState = ScObjType.GetState(index);
			throw new NotImplementedException();
			//TODO: Subscribe to new state's event subscriptions (if valid). Run new state Start method.
		}


		// Serialization?


	}
}
