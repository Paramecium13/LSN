using Syroot.BinaryData;
using System.Collections.Generic;
using LsnCore.Expressions;
using System.Collections;

namespace LsnCore.Statements
{
	public abstract class Statement : Component, IEnumerable<IExpression>
	{
		public abstract IEnumerator<IExpression> GetEnumerator();
		internal abstract void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override PreStatement[] Flatten(LabelInfo labelInfo)
		{
			var pre = new PreStatement(this) { Target = GetPreStatementTarget(labelInfo) };
			return new PreStatement[] { pre };
		}

		public virtual string GetPreStatementTarget(LabelInfo labelInfo) => null;
	}
}
