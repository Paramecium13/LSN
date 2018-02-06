using LsnCore.Types;
using System.Collections.Generic;
using System.Linq;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class RecordConstructor : Expression
	{
		public readonly IExpression[] Args;

		public override bool IsPure => Args.All(a => a.IsPure);

		public RecordConstructor(RecordType type, IDictionary<string,IExpression> args)
		{
			Args = new IExpression[type.FieldCount];
			var i = -1;
			foreach (var pair in args)
			{
				i = type.GetIndex(pair.Key);
				Args[i] = pair.Value;
			}
		}

		public RecordConstructor(TypeId type, IEnumerable<IExpression> args)
		{
			Type = type;
			Args = args.ToArray();
		}

		public override LsnValue Eval(IInterpreter i)
		//=> new StructValue(_Type, ArgsB.Select(e => e.Eval(i)).ToArray());
		{
			var length = Args.Length;
			var values = new LsnValue[length];
			for(int j = 0; j < length; j++)
			{
				values[j] = Args[j].Eval(i);
			}
			return new LsnValue(new RecordValue(values));
		}

		public override IExpression Fold()
		{
			var args = Args.Select(a => a.Fold()).ToArray();
			return new RecordConstructor(Type, args);
		}

		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.RecordConstructor);

			resourceSerializer.WriteTypeId(Type, writer);

			writer.Write((ushort)Args.Length);
			for (int i = 0; i < Args.Length; i++)
				Args[i].Serialize(writer, resourceSerializer);
		}
	}
}
