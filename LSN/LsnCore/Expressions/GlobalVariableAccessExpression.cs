using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class GlobalVariableAccessExpression : IExpression
	{
		//private readonly string GlobalVarFileName;
		private readonly string GlobalVarName;

		public bool IsPure => false;

		private readonly TypeId _Type;
		public TypeId Type => _Type;

		public GlobalVariableAccessExpression(string gvarName, TypeId type)
		{
			GlobalVarName = gvarName; _Type = type;
		}

		public bool Equals(IExpression other)
		{
			var gl = other as GlobalVariableAccessExpression;
			if (gl == null) return false;
			return gl.GlobalVarName == GlobalVarName;
		}

		public LsnValue Eval(IInterpreter i) => throw new NotImplementedException();//i.GetGlobalVariable(GlobalVarName/*, GlobalVarFileName*/);

		public IExpression Fold() => this;

		public bool IsReifyTimeConst() => false;

		public void Replace(IExpression oldExpr, IExpression newExpr){}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}
	}
}
