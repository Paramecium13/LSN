using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	[Serializable]
	public sealed class UniqueScriptObjectAccessExpression : Expression
	{
		private readonly string Name;

		public override bool IsPure => false;

		public UniqueScriptObjectAccessExpression(string name, TypeId type)
		{
			Name = name; Type = type;
		}

		public UniqueScriptObjectAccessExpression(string name)
		{
			Name = name;
		}

		public override LsnValue Eval(IInterpreter i)
			=> new LsnValue(i.GetUniqueScriptObject(Name));

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.UniqueScriptObjectAccess);
			writer.Write(Name);
		}
	}
}
