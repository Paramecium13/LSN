using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Collections;
using LSNr;
using LSNr.CodeGeneration;

namespace LsnCore.Expressions
{
	public sealed class GlobalVariableAccessExpression : IExpression
	{
		//private readonly string GlobalVarFileName;
		private readonly string GlobalVarName;

		public bool IsPure => false;

		public TypeId Type { get; }

		public GlobalVariableAccessExpression(string gvarName, TypeId type)
		{
			GlobalVarName = gvarName; Type = type;
		}

		public bool Equals(IExpression other)
		{
			if (other is not GlobalVariableAccessExpression gl) return false;
			return gl.GlobalVarName == GlobalVarName;
		}

		/// <inheritdoc />
		public void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}

#if CORE
		public LsnValue Eval(IInterpreter i) => throw new NotImplementedException();//i.GetGlobalVariable(GlobalVarName/*, GlobalVarFileName*/);
#endif

		public IExpression Fold() => this;

		public bool IsReifyTimeConst() => false;

		public void Replace(IExpression oldExpr, IExpression newExpr){}

		public void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<IExpression> GetEnumerator()
		{
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
