using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using LsnCore.Values;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class HostInterfaceMethodCall : IExpression
	{
		/// <inheritdoc />
		public bool IsPure => false;

		/// <inheritdoc />
		public TypeId Type { get; }

		internal string Name => Signature.Name;

		private IExpression HostInterface;

		private readonly IExpression[] Arguments;

		internal FunctionSignature Signature { get; }

		public HostInterfaceMethodCall(FunctionSignature def, IExpression hostInterface, IExpression[] args)
		{
			Type = def.ReturnType; HostInterface = hostInterface; Arguments = args; Signature = def;
		}

		public bool Equals(IExpression other) => this == other;

#if CORE
		public LsnValue Eval(IInterpreter i)
		{
			return (HostInterface.Eval(i).Value as IHostInterface).CallMethod(Name, Arguments.Select(a => a.Eval(i)).ToArray());
		}
#endif

		/// <inheritdoc />
		public IExpression Fold()
		{
			HostInterface = HostInterface.Fold();
			for (int i = 0; i < Arguments.Length; i++)
				Arguments[i] = Arguments[i].Fold();

			return this;
		}

		/// <inheritdoc />
		public bool IsReifyTimeConst() => false;

		/// <inheritdoc />
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

		/// <inheritdoc />
		public IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			HostInterface.GetInstructions(instructions, context.WithContext(ExpressionContext.SubExpression));
			var paramContext = context.WithContext(ExpressionContext.Parameter_Default);
			foreach (var argument in Arguments)
			{
				argument.GetInstructions(instructions, paramContext);
			}

			instructions.AddInstruction(new HostInterfaceMethodCallPreInstruction(Signature));
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
