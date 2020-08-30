using System;
using System.Collections.Generic;
using System.Linq;
using LSNr;
using MoreLinq;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class FunctionCall : Expression
	{
		public Function Fn;

		public override bool IsPure => false;

		//ToDo: Encapsulate? Used in ExpressionWalker...
		public readonly IExpression[] Args;

		public FunctionCall(Function fn, IExpression[] args)// This is possible because when a resource file is loaded, function stubs are created for
															// all its functions before any code is read. Thus when the reader reads the code, it has
															// a function object for every function that the code could call.
		{
			Fn = fn;
			Args = args;
			Type = fn.ReturnType;
		}

		public override IExpression Fold()
		{
			var args = Args.Select(a => a.Fold()).ToArray();
			return new FunctionCall(Fn, args);
		}

		public override bool IsReifyTimeConst() => false;

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if(Args.Contains(oldExpr))
				Args[Array.IndexOf(Args, oldExpr)] = newExpr;
		}

		public override bool Equals(IExpression other)
		{
			var e = other as FunctionCall;
			if (e == null) return false;
			return e.Args.SequenceEqual(Args);
		}

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.FunctionCall);
			writer.Write(Fn.Name);
			writer.Write((byte)Args.Length);
			foreach (var t in Args)
				t.Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			foreach (var arg in Args)
			{
				yield return arg;
				foreach (var expr in arg.SelectMany(e => e))
					yield return expr;
			}
		}

		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			return Args.SelectMany(a => a.GetInstructions(context)).Append(new FunctionCallPreInstruction(Fn));
		}
	}
}
