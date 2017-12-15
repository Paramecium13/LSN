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
	public sealed class StructConstructor : Expression
	{
		public readonly IExpression[] Args;

		public override bool IsPure => Args.All(a => a.IsPure);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public StructConstructor(StructType type, IDictionary<string, IExpression> args)
		{
			Type = type.Id;
			Args = new IExpression[type.FieldCount];
			int i = -1;
			foreach (var pair in args)
			{
				i = type.GetIndex(pair.Key);
				Args[i] = pair.Value;
			}
		}

		public StructConstructor(IEnumerable<IExpression> args)
		{
			Args = args.ToArray();
		}

		public override LsnValue Eval(IInterpreter i)
		{
			int length = Args.Length;
			LsnValue[] values = new LsnValue[length];
			for (int j = 0; j < length; j++)
			{
				values[j] = Args[j].Eval(i);
			}
			return new LsnValue(new StructValue(values));//....
		}

		public override IExpression Fold() //ToDo: Implement (but do not return a struct value).
		{//d = Args.Select(pair => new KeyValuePair<string, ILsnValue>(pair.Key, pair.Value as ILsnValue)).ToDictionary()
			/*var a = Args.Select(pair => new KeyValuePair<string, IExpression>(pair.Key, pair.Value.Fold())).ToDictionary();
			if (a.Values.All(v => v.IsReifyTimeConst() && v is LsnValue?))
				return new LsnValue(
					new StructValue(_Type, Args.Select(pair 
					=> new KeyValuePair<string,LsnValue>(pair.Key,(LsnValue)pair.Value)).ToDictionary())
					);
			else
				return new StructConstructor(_Type, a);*/
			return this;
		}

		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.StructConstructor);
			writer.Write((ushort)Args.Length);
			for (int i = 0; i < Args.Length; i++)
				Args[i].Serialize(writer, resourceSerializer);
		}
	}
}
