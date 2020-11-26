using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class StructConstructor : Expression
	{
		public readonly IExpression[] Args;

		/// <inheritdoc />
		public override bool IsPure => Args.All(a => a.IsPure);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public StructConstructor(StructType type, IDictionary<string, IExpression> args)
		{
			Type = type.Id;
			Args = new IExpression[type.FieldCount];
			foreach (var pair in args)
			{
				var i = type.GetIndex(pair.Key);
				Args[i] = pair.Value;
			}
		}

		public StructConstructor(TypeId type, IExpression[] args)
		{
			Type = type; Args = args;
		}

		/// <inheritdoc />
		public override IExpression Fold()
		{
			var a = Args.Select(x => x.Fold()).ToArray();
			/*if (a.All(v => v.IsReifyTimeConst() && v is LsnValue?))
				return new LsnValue(new StructValue(Type, a.Cast<LsnValue>().ToArray()));*/
			return new StructConstructor(Type, a);
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			var subcontext = context.WithContext(ExpressionContext.Store);
			foreach (var arg in Args)
			{
				arg.GetInstructions(instructions, subcontext);
			}

			instructions.AddInstruction(new TypeTargetedInstruction(OpCode.ConstructStruct, Type));
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst() => false;

		/// <inheritdoc />
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.StructConstructor);

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
