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
		public readonly IDictionary<string, IExpression> Args; //ToDo: Remove

		public readonly IExpression[] ArgsB;


		public override bool IsPure => ArgsB.All(a => a.IsPure);


		[NonSerialized]
		private readonly RecordType _Type;
		//public readonly TypeId Id;

		//public override TypeId Type => Id;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public RecordConstructor(RecordType type, IDictionary<string, IExpression> args)
		{
			Type = type.Id;
			_Type = type; Args = args;
			ArgsB = new IExpression[_Type.FieldCount];
			int i = -1;
			foreach (var pair in args)
			{
				i = type.GetIndex(pair.Key);
				ArgsB[i] = pair.Value;
			}
		}


		public override LsnValue Eval(IInterpreter i)
		{
			int length = ArgsB.Length;
			LsnValue[] values = new LsnValue[length];
			for (int j = 0; j < length; j++)
			{
				values[j] = ArgsB[j].Eval(i);
			}
			return new LsnValue(new RecordValue(Type, values));
		}

		public override IExpression Fold()
		{//d = Args.Select(pair => new KeyValuePair<string, ILsnValue>(pair.Key, pair.Value as ILsnValue)).ToDictionary()
			var a = Args.Select(pair => new KeyValuePair<string, IExpression>(pair.Key, pair.Value.Fold())).ToDictionary();
			if (a.Values.All(v => v.IsReifyTimeConst() && v is LsnValue?))
				return new LsnValue(
					new RecordValue(_Type, Args.Select(pair 
					=> new KeyValuePair<string,LsnValue>(pair.Key,(LsnValue)pair.Value)).ToDictionary())
					);
			else
				return new RecordConstructor(_Type, a);
		}


		public override bool IsReifyTimeConst() => false;

	}
}
