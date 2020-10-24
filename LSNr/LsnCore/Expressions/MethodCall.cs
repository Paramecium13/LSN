using System.Collections.Generic;
using System.Linq;
using LsnCore.Types;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class MethodCall : Expression
	{
		public IExpression[] Args;

		public override bool IsPure => false;

		public readonly Method Method;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MethodCall(Method m, IExpression[] args)
		{
			Args = args ?? new IExpression[1];
			Method = m;
			Type = m.ReturnType;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MethodCall(string name, IExpression[] args, LsnType callingType)
		{
			Args = args;
			Method = callingType.Methods[name];
			Type = Method.ReturnType;
		}

#if CORE
		public override LsnValue Eval(IInterpreter i)
		{
			var args = new LsnValue[Args.Length];
			for (int x = 0; x < Args.Length; x++)
				args[x] = Args[x].Eval(i);
			return Method.Eval(args, i);
		}
#endif

		/// <inheritdoc/>
		public override IExpression Fold()
		{
			for (int i = 0; i < Args.Length; i++)
			{
				Args[i] = Args[i].Fold();
			}

			return this;
		}

		/// <inheritdoc />
		public override IEnumerable<PreInstruction> GetInstructions(InstructionGenerationContext context)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			Args[0].GetInstructions(instructions, context.WithContext(ExpressionContext.MethodCall));
			var subContext = context.WithContext(ExpressionContext.Parameter_Default);
			for (var index = 1; index < Args.Length; index++)
			{
				Args[index].GetInstructions(instructions, subContext);
			}

			switch (Method)
			{
				case InstructionMappedMethod mapped:
					instructions.AddInstruction(new SimplePreInstruction(mapped.OpCode, mapped.Data));
					return;
				case BoundedMethod bound:
					instructions.AddInstruction(new TypeTargetedInstruction(OpCode.LoadTempIndex, bound.TypeId));
					instructions.AddInstruction(
						new IdStringTargetedPreInstruction(OpCode.CallNativeMethod, bound.Name));
					return;
				case ScriptClassVirtualMethod virtualMethod:
					throw new System.NotImplementedException();
				case ScriptClassMethod scMethod:
					instructions.AddInstruction(new FunctionCallPreInstruction(scMethod));
					return;
				default:
					throw new System.NotImplementedException();
			}
		}

		/// <inheritdoc/>
		public override bool IsReifyTimeConst() => false;

		/// <inheritdoc/>
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.MethodCall);
			var type = Method.TypeId ?? Method.Parameters[0].Type;
			resourceSerializer.WriteTypeId(type, writer);
			writer.Write(Method.Name);
			writer.Write((byte)Args.Length);
			for (int i = 0; i < Args.Length; i++)
				Args[i].Serialize(writer, resourceSerializer);
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
	}
}
