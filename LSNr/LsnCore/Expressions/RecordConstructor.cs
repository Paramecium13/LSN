using LsnCore.Types;
using System.Collections.Generic;
using System.Linq;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class RecordConstructor : Expression
	{
		public readonly IExpression[] Args;

		/// <inheritdoc />
		public override bool IsPure => Args.All(a => a.IsPure);

		public RecordConstructor(RecordType type, IDictionary<string,IExpression> args)
		{
			Args = new IExpression[type.FieldCount];
			foreach (var pair in args)
			{
				var i = type.GetIndex(pair.Key);
				Args[i] = pair.Value;
			}

			Type = type.Id;
		}

		public RecordConstructor(TypeId type, IEnumerable<IExpression> args)
		{
			Type = type;
			Args = args.ToArray();
		}

		/// <inheritdoc />
		public override IExpression Fold()
		{
			var args = Args.Select(a => a.Fold()).ToArray();
			return new RecordConstructor(Type, args);
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			var subcontext = context.WithContext(ExpressionContext.Store);
			foreach (var arg in Args)
			{
				arg.GetInstructions(instructions, subcontext);
			}

			instructions.AddInstruction(new TypeTargetedInstruction(OpCode.ConstructRecord, Type));
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst() => false;

		/// <inheritdoc />
		public override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.RecordConstructor);

			resourceSerializer.WriteTypeId(Type, writer);

			writer.Write((ushort)Args.Length);
			for (var i = 0; i < Args.Length; i++)
				Args[i].Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc />
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
