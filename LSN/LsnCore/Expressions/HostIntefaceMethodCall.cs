using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	[Serializable]
	public sealed class HostInterfaceMethodCall : IExpression
	{
		public bool IsPure => false;

		private readonly TypeId _Type;
		public TypeId Type => _Type;

		private readonly string Name;

		private IExpression HostInterface;

		private IExpression[] Arguments;


		public HostInterfaceMethodCall(FunctionSignature def, IExpression hostInterface, IExpression[] args)
		{
			_Type = def.ReturnType; Name = def.Name; HostInterface = hostInterface; Arguments = args;
		}

		public HostInterfaceMethodCall(string name, IExpression hostInterface, IExpression[] args)
		{
			Name = name; HostInterface = hostInterface; Arguments = args;
		}

		public bool Equals(IExpression other) => this == other;

		public LsnValue Eval(IInterpreter i)
		{
			return (HostInterface.Eval(i).Value as IHostInterface).CallMethod(Name, Arguments.Select(a => a.Eval(i)).ToArray());
		}

		public IExpression Fold()
		{
			HostInterface = HostInterface.Fold();
			for (int i = 0; i < Arguments.Length; i++)
				Arguments[i] = Arguments[i].Fold();

			return this;
		}

		public bool IsReifyTimeConst() => false;

		public void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (HostInterface == oldExpr)
				HostInterface = newExpr;
			for (int i = 0; i < Arguments.Length; i++)
				if (Arguments[i] == oldExpr) Arguments[i] = newExpr;
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.HostInterfaceMethodCall);
			writer.Write(Name);
			HostInterface.Serialize(writer, resourceSerializer);
			writer.Write((byte)Arguments.Length);
			for (int i = 0; i < Arguments.Length; i++)
				Arguments[i].Serialize(writer, resourceSerializer);
		}
	}
}
