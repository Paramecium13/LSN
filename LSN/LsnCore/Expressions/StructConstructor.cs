using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Expressions
{
	[Serializable]
	public class StructConstructor : IExpression
	{

		private readonly IDictionary<string, IExpression> Args;

		private readonly LsnStructType _Type;


		public StructConstructor(LsnStructType type, IDictionary<string,IExpression> args)
		{
			_Type = type; Args = args;
		}

		public LsnType Type => _Type;

		public ILsnValue Eval(IInterpreter i)
			=> new StructValue(_Type, Args.Select(p => new KeyValuePair<string, ILsnValue>(p.Key, p.Value.Eval(i))).ToDictionary());

		public IExpression Fold()
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

		public bool IsReifyTimeConst() => false;

	}
}
