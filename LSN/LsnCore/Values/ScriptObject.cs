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

		private readonly LsnValue[] Fields;

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


		internal ScriptObjectMethod GetMethod(string methodName)
		{
			throw new NotImplementedException();
		}

		internal void SetState(string state)
		{
			throw new NotImplementedException();
		}

	}
}
