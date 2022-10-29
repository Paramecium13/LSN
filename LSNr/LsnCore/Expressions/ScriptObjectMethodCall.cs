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
	/*public class ScriptObjectVirtualMethodCall : Expression
	{
		private readonly IExpression[] Parameters;

		//private readonly bool IsVirtual;

		private readonly string Name;

		public override bool IsPure => false;

		public ScriptObjectVirtualMethodCall(IExpression[] parameters, string name, TypeId type)
		{
			Parameters = parameters; Name = name; Type = type;
		}

		public ScriptObjectVirtualMethodCall(IExpression[] parameters, string name)
		{
			Parameters = parameters; Name = name;
		}

#if CORE
		public override LsnValue Eval(IInterpreter i)
		{
			var parameters = Parameters.Select(p => p.Eval(i)).ToArray();
			return (parameters[0].Value as ScriptObject).GetMethod(Name).Eval(parameters, i);
		}
#endif

		public override IExpression Fold()
			=> new ScriptObjectVirtualMethodCall(Parameters.Select(p => p.Fold()).ToArray(), Name, Type);

		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.ScriptObjectMethodCall);
			writer.Write(Name);
			writer.Write((byte)Parameters.Length);
			for (int i = 0; i < Parameters.Length; i++)
				Parameters[i].Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			foreach (var arg in Parameters)
			{
				yield return arg;
				foreach (var expr in arg.SelectMany(e => e))
					yield return expr;
			}
		}
	}*/
}
