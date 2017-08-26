using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public class StructConstructor : Expression
	{

		public readonly IDictionary<string, IExpression> Args; //ToDo: remove.

		public readonly IExpression[] ArgsB;

		public override bool IsPure => ArgsB.All(a => a.IsPure);

		//ToDo: make non-serialized
		private readonly RecordType _Type;
		
		public StructConstructor(RecordType type, IDictionary<string,IExpression> args)
		{
			_Type = type; Args = args; Type = type.Id;
			ArgsB = new IExpression[_Type.FieldCount];
			int i = -1;
			foreach (var pair in args)
			{
				i = type.GetIndex(pair.Key);
				ArgsB[i] = pair.Value;
			}
		}

		public override LsnValue Eval(IInterpreter i)
		//=> new StructValue(_Type, ArgsB.Select(e => e.Eval(i)).ToArray());
		{
			int length = ArgsB.Length;
			LsnValue[] values = new LsnValue[length];
			for(int j = 0; j < length; j++)
			{
				values[j] = ArgsB[j].Eval(i);
			}
			return new LsnValue(new RecordValue(values, Type));
		}

		public override IExpression Fold()
		{
			var a = Args.Select(pair => new KeyValuePair<string, IExpression>(pair.Key, pair.Value.Fold())).ToDictionary();
			if (a.Values.All(v => v.IsReifyTimeConst() && v is LsnValue?))
				return new LsnValue(
					new RecordValue(_Type, Args.Select(pair
					=> new KeyValuePair<string, LsnValue>(pair.Key, (LsnValue)pair.Value)).ToDictionary())
					);
			else
				return new StructConstructor(_Type, a);
		}

		public override bool IsReifyTimeConst() => false;

	}
}
