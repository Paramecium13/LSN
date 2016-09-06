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

		public readonly IDictionary<string, IExpression> Args;

		public readonly IExpression[] ArgsB;

		private readonly LsnStructType _Type;

		public readonly TypeId Id;

		public override TypeId Type => Id;


		public StructConstructor(LsnStructType type, IDictionary<string,IExpression> args)
		{
			_Type = type; Args = args;
			ArgsB = new IExpression[_Type.FieldCount];
			int i = -1;
			foreach (var pair in args)
			{
				i = type.GetIndex(pair.Key);
				ArgsB[i] = pair.Value;
			}
		}

		public override ILsnValue Eval(IInterpreter i)
		//=> new StructValue(_Type, ArgsB.Select(e => e.Eval(i)).ToArray());
		{
			int length = ArgsB.Length;
			ILsnValue[] values = new ILsnValue[length];
			for(int j = 0; j < length; j++)
			{
				values[j] = ArgsB[j].Eval(i);
			}
			return new StructValue(values,Id);
		}

		public override IExpression Fold()
		{
			IDictionary<string, ILsnValue> d;
			var a = Args.Select(pair => new KeyValuePair<string, IExpression>(pair.Key, pair.Value.Fold())).ToDictionary();
			if (a.Values.All(v => v.IsReifyTimeConst()) && 
				(d = Args.Select(pair => new KeyValuePair<string, ILsnValue>(pair.Key, pair.Value as ILsnValue)).ToDictionary())
				.All(p => p.Value != null))
				return new StructValue(_Type, d);
			
			else
				return new StructConstructor(_Type, a);		
		}

		public override bool IsReifyTimeConst() => false;

	}
}
