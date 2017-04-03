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


		private int CurrentState;


		public ScriptObject(LsnValue[] properties, LsnValue[] fields, ScriptObjectType type, int currentState, IHostInterface host = null)
		{
			Properties = properties; Fields = fields; Type = type.Id; CurrentState = currentState;
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
			//		Run the current state's implementation
			//		(END)
			//	Else Does the Type have an implementation?
			//		Run that
			//		(END)
			//	Else
			//	Throw exception
			//Else
			//	Run the type's implementation.
			throw new NotImplementedException();
		}


		internal void SetState(int index)
		{
			//TODO: Unsubscribe from old state's event subscriptions (if valid). Run old state exit method.
			CurrentState = index;
			throw new NotImplementedException();
			//TODO: Subscribe to new state's event subscriptions (if valid). Run new state Start method.
		}


		// Serialization?


	}
}
