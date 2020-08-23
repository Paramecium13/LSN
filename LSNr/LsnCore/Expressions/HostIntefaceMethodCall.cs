using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class HostInterfaceMethodCall : IExpression
	{
		public bool IsPure => false;

		public TypeId Type { get; }

		private readonly string Name;

		private IExpression HostInterface;

		private readonly IExpression[] Arguments;


		public HostInterfaceMethodCall(FunctionSignature def, IExpression hostInterface, IExpression[] args)
		{
			Type = def.ReturnType; Name = def.Name; HostInterface = hostInterface; Arguments = args;
		}

		public HostInterfaceMethodCall(string name, IExpression hostInterface, IExpression[] args)
		{
			Name = name; HostInterface = hostInterface; Arguments = args;
		}

		public bool Equals(IExpression other) => this == other;

#if CORE
		public LsnValue Eval(IInterpreter i)
		{
			return (HostInterface.Eval(i).Value as IHostInterface).CallMethod(Name, Arguments.Select(a => a.Eval(i)).ToArray());
		}
#endif

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

		public IEnumerator<IExpression> GetEnumerator()
		{
			yield return HostInterface;
			foreach (var expr in HostInterface.SelectMany(e => e))
				yield return expr;
			foreach (var arg in Arguments)
			{
				yield return arg;
				foreach (var expr in arg.SelectMany(e => e))
					yield return expr;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
