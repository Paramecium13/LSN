using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public class RecordConstructor : Expression
	{
		private readonly IDictionary<string, IExpression> Args;

		private readonly RecordType _Type;


		public RecordConstructor(RecordType type, IDictionary<string, IExpression> args)
		{
			Type = type; Args = args;
		}

		public override LsnType Type => _Type;

		public override ILsnValue Eval(IInterpreter i)
			=> new RecordValue(_Type, Args.Select(p => new KeyValuePair<string, ILsnValue>(p.Key, p.Value.Eval(i))).ToDictionary());

		public override IExpression Fold()
		{
			IDictionary<string, ILsnValue> d;
			var a = Args.Select(pair => new KeyValuePair<string, IExpression>(pair.Key, pair.Value.Fold())).ToDictionary();
			if (a.Values.All(v => v.IsReifyTimeConst()) &&
				(d = Args.Select(pair => new KeyValuePair<string, ILsnValue>(pair.Key, pair.Value as ILsnValue)).ToDictionary())
				.All(p => p.Value != null))
				return new RecordValue(_Type, d);

			else
				return new RecordConstructor(_Type, a);
		}

		public override bool IsReifyTimeConst() => false;

	}
}
